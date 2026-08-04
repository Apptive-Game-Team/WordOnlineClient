using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Protocol;

namespace GameScene.Simulation.Resources
{
    public enum CastResultCode { Success, UnknownPlayer, MissingCard, InsufficientMana, InvalidMagic }
    public enum SimulationResult { Ongoing, LeftWin, RightWin, Draw }

    public readonly struct CastResult
    {
        public long UserId { get; }
        public int RequestId { get; }
        public CastResultCode Code { get; }
        public CastResult(long userId, int requestId, CastResultCode code)
        { UserId = userId; RequestId = requestId; Code = code; }
    }

    public sealed class PlayerResourceState
    {
        private readonly List<string> deck;
        private readonly List<string> hand = new List<string>();

        public long UserId { get; }
        public int Mana { get; internal set; }
        public int Health { get; internal set; } = 100;
        public IReadOnlyList<string> Hand => hand;

        internal PlayerResourceState(long userId, IEnumerable<string> orderedDeck)
        { UserId = userId; deck = new List<string>(orderedDeck); }

        internal void Draw(int maxHand)
        {
            if (hand.Count >= maxHand || deck.Count == 0) return;
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }

        internal bool ContainsAll(IReadOnlyList<string> cards)
        {
            List<string> available = new List<string>(hand);
            for (int i = 0; i < cards.Count; i++) if (!available.Remove(cards[i])) return false;
            return true;
        }

        internal void ConsumeAndReturn(IReadOnlyList<string> cards)
        {
            for (int i = 0; i < cards.Count; i++) hand.Remove(cards[i]);
            for (int i = 0; i < cards.Count; i++) deck.Add(cards[i]);
        }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteInt64(UserId); writer.WriteInt32(Mana); writer.WriteInt32(Health);
            writer.WriteInt32(deck.Count);
            for (int i = 0; i < deck.Count; i++) writer.WriteString(deck[i]);
            writer.WriteInt32(hand.Count);
            for (int i = 0; i < hand.Count; i++) writer.WriteString(hand[i]);
        }
    }

    public sealed class ResourceSimulation
    {
        private readonly DeterministicGameRules rules;
        private readonly List<CastResult> castResults = new List<CastResult>();
        private Fix64 leftManaChargePercent;
        private Fix64 rightManaChargePercent;
        public PlayerResourceState Left { get; }
        public PlayerResourceState Right { get; }
        public int FrameNumber { get; private set; }
        public int RemainingFrames { get; private set; }
        public SimulationResult Result { get; private set; } = SimulationResult.Ongoing;
        public IReadOnlyList<CastResult> LastCastResults => castResults;

        public ResourceSimulation(DeterministicGameRules rules, long leftUserId, long rightUserId,
            IEnumerable<string> leftDeck, IEnumerable<string> rightDeck)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            Left = new PlayerResourceState(leftUserId, leftDeck);
            Right = new PlayerResourceState(rightUserId, rightDeck);
            RemainingFrames = rules.DurationFrames;
            ApplyPassive(0, Left); ApplyPassive(0, Right);
        }

        public void Step(ConfirmedFrameMessage frame)
        {
            if (Result != SimulationResult.Ongoing) throw new InvalidOperationException("Match already ended");
            if (frame == null || frame.frameNum != FrameNumber + 1)
                throw new InvalidOperationException("Unexpected resource frame");
            castResults.Clear();
            ApplyPassive(frame.frameNum, Left); ApplyPassive(frame.frameNum, Right);
            ConfirmedInputMessage[] inputs = frame.inputs ?? Array.Empty<ConfirmedInputMessage>();
            for (int i = 0; i < inputs.Length; i++) ApplyInput(inputs[i]);
            FrameNumber = frame.frameNum;
            RemainingFrames = Math.Max(0, RemainingFrames - 1);
            if (RemainingFrames == 0)
            {
                if (Left.Health < Right.Health) Result = SimulationResult.RightWin;
                else if (Right.Health < Left.Health) Result = SimulationResult.LeftWin;
                else Result = SimulationResult.Draw;
            }
        }

        public ulong CalculateStateHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter();
            writer.WriteInt32(FrameNumber); writer.WriteInt32(RemainingFrames); writer.WriteInt32((int)Result);
            writer.WriteFixed64(leftManaChargePercent); writer.WriteFixed64(rightManaChargePercent);
            Left.WriteState(writer); Right.WriteState(writer);
            return writer.Hash;
        }

        public PlayerResourceSnapshot CreatePlayerSnapshot(long userId)
        {
            PlayerResourceState state = userId == Left.UserId ? Left
                : userId == Right.UserId ? Right : null;
            if (state == null) throw new InvalidOperationException("Unknown player resource snapshot: " + userId);
            return new PlayerResourceSnapshot(state, RemainingFrames);
        }

        public void ResolveCombatHealth(int leftHealth, int rightHealth)
        {
            Left.Health = Math.Max(0, leftHealth);
            Right.Health = Math.Max(0, rightHealth);
            if (Result != SimulationResult.Ongoing) return;
            if (Left.Health == 0 && Right.Health == 0) Result = SimulationResult.Draw;
            else if (Left.Health == 0) Result = SimulationResult.RightWin;
            else if (Right.Health == 0) Result = SimulationResult.LeftWin;
        }

        public void SetManaChargePercent(long userId, Fix64 percent)
        {
            if (percent < Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(percent));
            if (userId == Left.UserId) leftManaChargePercent = percent;
            else if (userId == Right.UserId) rightManaChargePercent = percent;
            else throw new InvalidOperationException("Unknown mana charge player: " + userId);
        }

        private void ApplyPassive(int frame, PlayerResourceState player)
        {
            bool fever = rules.FeverDurationFrames > 0 &&
                RemainingFrames < rules.FeverDurationFrames;
            if (frame % rules.ManaChargeIntervalFrames == 0)
            {
                Fix64 percent = player.UserId == Left.UserId ? leftManaChargePercent : rightManaChargePercent;
                int scaledPercent = checked((int)(percent * (Fix64)1000 + (Fix64)0.5m));
                int baseAmount = fever ? checked(rules.ManaChargeAmount * 2) : rules.ManaChargeAmount;
                int amount = checked(baseAmount * (1000 + scaledPercent) / 1000);
                player.Mana = Math.Min(rules.MaxMana, player.Mana + amount);
            }
            int drawInterval = fever ? Math.Max(1, rules.CardDrawIntervalFrames / 2)
                : rules.CardDrawIntervalFrames;
            if (frame % drawInterval == 0) player.Draw(rules.MaxHandSize);
        }

        private void ApplyInput(ConfirmedInputMessage confirmed)
        {
            FrameInputMessage input = confirmed?.input;
            if (input == null || !string.Equals(input.type, "useMagic", StringComparison.Ordinal)) return;
            PlayerResourceState player = confirmed.userId == Left.UserId ? Left
                : confirmed.userId == Right.UserId ? Right : null;
            if (player == null) { castResults.Add(new CastResult(confirmed.userId, input.id, CastResultCode.UnknownPlayer)); return; }
            string[] cards = input.cards ?? Array.Empty<string>();
            if (!player.ContainsAll(cards)) { castResults.Add(new CastResult(player.UserId, input.id, CastResultCode.MissingCard)); return; }
            int cost = 0; for (int i = 0; i < cards.Length; i++) cost = checked(cost + rules.ManaCost(cards[i]));
            if (cost > player.Mana) { castResults.Add(new CastResult(player.UserId, input.id, CastResultCode.InsufficientMana)); return; }
            if (!rules.IsMagic(cards))
            {
                player.Mana -= cost; player.ConsumeAndReturn(cards);
                castResults.Add(new CastResult(player.UserId, input.id, CastResultCode.InvalidMagic)); return;
            }
            player.Mana -= cost; player.ConsumeAndReturn(cards);
            castResults.Add(new CastResult(player.UserId, input.id, CastResultCode.Success));
        }
    }
}

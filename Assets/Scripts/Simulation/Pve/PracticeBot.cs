using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Protocol;

namespace GameScene.Simulation.Pve
{
    public readonly struct PracticeBotRecipe
    {
        private readonly string[] cards;
        public IReadOnlyList<string> Cards => cards;
        public Fix64 Range { get; }
        public int Cost { get; }
        public bool Offensive { get; }

        public PracticeBotRecipe(IEnumerable<string> cards, Fix64 range, int cost, bool offensive)
        {
            this.cards = new List<string>(cards ?? throw new ArgumentNullException(nameof(cards))).ToArray();
            if (this.cards.Length == 0) throw new ArgumentException("Bot recipe cards required", nameof(cards));
            if (range < Fix64.Zero || cost < 0) throw new ArgumentOutOfRangeException(nameof(range));
            Range = range;
            Cost = cost;
            Offensive = offensive;
        }
    }

    public sealed class PracticeBot
    {
        private sealed class PendingDecision
        {
            public int ReadyFrame;
            public ConfirmedInputMessage Input;
        }

        private readonly long botUserId;
        private readonly int reactionIntervalFrames;
        private readonly int thinkingDelayFrames;
        private readonly PracticeBotRecipe[] recipes;
        private readonly string[] deckCards;
        private readonly Dictionary<string, int> cardCosts;
        private readonly Dictionary<string, Fix64> cardRanges;
        private readonly DeterministicRandom random;
        private readonly int tierCandidateWindow;
        private readonly int counterAggressionPermille;
        private PendingDecision pending;
        private int sequence;

        public PracticeBot(long sessionSeed, long botUserId, int reactionIntervalFrames, params string[] cards)
            : this(sessionSeed, botUserId, reactionIntervalFrames, 0,
                ToRecipes(SingleCardRecipes(cards)), cards, null, null, "ELITE", 0)
        {
        }

        public PracticeBot(long sessionSeed, long botUserId, int reactionIntervalFrames,
            IEnumerable<IEnumerable<string>> validRecipes, params string[] cards)
            : this(sessionSeed, botUserId, reactionIntervalFrames, 0,
                ToRecipes(validRecipes), cards, null, null, "ELITE", 0)
        {
        }

        public PracticeBot(long sessionSeed, long botUserId, int reactionIntervalFrames,
            int thinkingDelayFrames, IEnumerable<PracticeBotRecipe> validRecipes,
            IEnumerable<string> cards, IReadOnlyDictionary<string, int> cardCosts = null,
            IReadOnlyDictionary<string, Fix64> cardRanges = null,
            string tier = "ELITE", double counterAggression = 0)
        {
            if (botUserId >= 0) throw new ArgumentOutOfRangeException(nameof(botUserId));
            if (reactionIntervalFrames <= 0 || thinkingDelayFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(reactionIntervalFrames));
            deckCards = new List<string>(cards ?? throw new ArgumentNullException(nameof(cards))).ToArray();
            if (deckCards.Length == 0) throw new ArgumentException("Bot cards required", nameof(cards));
            List<PracticeBotRecipe> normalized = new List<PracticeBotRecipe>();
            foreach (PracticeBotRecipe recipe in validRecipes ?? throw new ArgumentNullException(nameof(validRecipes)))
                if (ContainsAll(deckCards, recipe.Cards)) normalized.Add(recipe);
            if (normalized.Count == 0) throw new ArgumentException("Bot requires one valid magic recipe", nameof(validRecipes));
            normalized.Sort(CompareRecipes);
            this.botUserId = botUserId;
            this.reactionIntervalFrames = reactionIntervalFrames;
            this.thinkingDelayFrames = thinkingDelayFrames;
            recipes = normalized.ToArray();
            this.cardCosts = Copy(cardCosts);
            this.cardRanges = Copy(cardRanges);
            tierCandidateWindow = CandidateWindow(tier);
            counterAggressionPermille = NormalizeAggression(counterAggression);
            random = new DeterministicRandom(unchecked(sessionSeed ^ botUserId));
        }

        public ConfirmedInputMessage Decide(int frameNumber, SimulationWorld world,
            IReadOnlyList<string> availableCards = null, int mana = int.MaxValue)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (frameNumber < 0) return null;
            if (pending != null)
            {
                if (frameNumber < pending.ReadyFrame) return null;
                ConfirmedInputMessage ready = pending.Input;
                pending = null;
                return ready;
            }
            if (frameNumber % reactionIntervalFrames != 0) return null;
            IReadOnlyList<string> hand = availableCards ?? deckCards;
            ConfirmedInputMessage decision = Think(world, hand, mana);
            if (decision == null) return null;
            if (thinkingDelayFrames == 0) return decision;
            pending = new PendingDecision
                { ReadyFrame = checked(frameNumber + thinkingDelayFrames), Input = decision };
            return null;
        }

        public ulong CalculateStateHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter();
            writer.WriteInt64(botUserId);
            writer.WriteInt32(reactionIntervalFrames);
            writer.WriteInt32(thinkingDelayFrames);
            writer.WriteInt32(tierCandidateWindow);
            writer.WriteInt32(counterAggressionPermille);
            writer.WriteInt32(sequence);
            random.WriteState(writer);
            writer.WriteBoolean(pending != null);
            if (pending != null)
            {
                writer.WriteInt32(pending.ReadyFrame);
                FrameInputMessage input = pending.Input.input;
                writer.WriteInt32(input.sequence);
                writer.WriteInt32(input.id);
                writer.WriteInt32(input.cards?.Length ?? 0);
                if (input.cards != null)
                    for (int index = 0; index < input.cards.Length; index++) writer.WriteString(input.cards[index]);
                writer.WriteInt32(FloatBits(input.position.x));
                writer.WriteInt32(FloatBits(input.position.y));
                writer.WriteInt32(FloatBits(input.position.z));
            }
            return writer.Hash;
        }

        private ConfirmedInputMessage Think(SimulationWorld world, IReadOnlyList<string> hand, int mana)
        {
            if (!TryFindBotPosition(world, out SimVector2 playerPosition)) return null;
            List<PracticeBotRecipe> makeable = new List<PracticeBotRecipe>();
            List<PracticeBotRecipe> affordable = new List<PracticeBotRecipe>();
            for (int index = 0; index < recipes.Length; index++)
            {
                PracticeBotRecipe recipe = recipes[index];
                if (!ContainsAll(hand, recipe.Cards)) continue;
                makeable.Add(recipe);
                if (recipe.Cost <= mana) affordable.Add(recipe);
            }

            List<SimulationEntity> enemies = Enemies(world);
            List<PracticeBotRecipe> usableOffensive = new List<PracticeBotRecipe>();
            for (int index = 0; index < affordable.Count; index++)
            {
                PracticeBotRecipe candidate = affordable[index];
                if (!candidate.Offensive) continue;
                SimulationEntity nearest = NearestInRange(playerPosition, enemies, candidate.Range);
                if (nearest == null) continue;
                usableOffensive.Add(candidate);
            }
            if (usableOffensive.Count > 0)
            {
                PracticeBotRecipe selected = SelectCandidate(usableOffensive, playerPosition, enemies, mana);
                SimulationEntity target = NearestInRange(playerPosition, enemies, selected.Range);
                return Input(selected.Cards, target.Position);
            }

            List<PracticeBotRecipe> placement = new List<PracticeBotRecipe>();
            for (int index = 0; index < affordable.Count; index++)
            {
                PracticeBotRecipe candidate = affordable[index];
                if (candidate.Offensive) continue;
                placement.Add(candidate);
            }
            if (placement.Count > 0)
            {
                PracticeBotRecipe selected = SelectCandidate(placement, playerPosition, enemies, mana);
                return Input(selected.Cards, RandomForwardPosition(playerPosition, enemies, selected.Range));
            }
            if (makeable.Count > 0 && affordable.Count == 0) return null;

            List<string> cycle = new List<string>();
            for (int index = 0; index < hand.Count; index++)
                if (Cost(hand[index]) <= mana) cycle.Add(hand[index]);
            if (cycle.Count == 0) return null;
            string card = cycle[random.NextInt(cycle.Count)];
            return Input(new[] { card }, RandomForwardPosition(playerPosition, enemies, Range(card)));
        }

        private ConfirmedInputMessage Input(IReadOnlyList<string> cards, SimVector2 position)
        {
            int requestSequence = sequence++;
            string[] copy = new string[cards.Count];
            for (int index = 0; index < cards.Count; index++) copy[index] = cards[index];
            return new ConfirmedInputMessage
            {
                userId = botUserId,
                input = new FrameInputMessage
                {
                    sequence = requestSequence,
                    type = "useMagic",
                    id = requestSequence,
                    cards = copy,
                    position = new ProtocolVector3
                    {
                        x = (float)(decimal)position.X,
                        y = 0,
                        z = (float)(decimal)position.Y
                    }
                }
            };
        }

        private SimVector2 RandomForwardPosition(SimVector2 playerPosition,
            IReadOnlyList<SimulationEntity> enemies, Fix64 range)
        {
            Fix64 x;
            Fix64 y;
            do
            {
                x = RandomSignedUnit();
                y = RandomSignedUnit();
            } while (x * x + y * y > Fix64.One || x == Fix64.Zero && y == Fix64.Zero);
            int direction = playerPosition.X > (Fix64)9 ? -1 : 1;
            SimulationEntity nearest = NearestInRange(playerPosition, enemies, Fix64.MaxValue);
            if (nearest != null)
                direction = nearest.Position.X < playerPosition.X ? -1 : 1;
            if (x < Fix64.Zero) x = -x;
            x *= direction;
            return new SimVector2(playerPosition.X + x * range, playerPosition.Y + y * range);
        }

        private Fix64 RandomSignedUnit() =>
            (Fix64)random.NextInt(2001) / (Fix64)1000 - Fix64.One;

        private bool TryFindBotPosition(SimulationWorld world, out SimVector2 position)
        {
            for (int index = 0; index < world.Entities.Count; index++)
            {
                SimulationEntity entity = world.Entities[index];
                if (!entity.IsDestroyed && entity.OwnerUserId == botUserId && entity.PrefabId == "Player")
                {
                    position = entity.Position;
                    return true;
                }
            }
            for (int index = 0; index < world.Entities.Count; index++)
            {
                SimulationEntity entity = world.Entities[index];
                if (!entity.IsDestroyed && entity.OwnerUserId >= 0 && entity.PrefabId == "Player")
                {
                    position = new SimVector2((Fix64)18 - entity.Position.X, entity.Position.Y);
                    return true;
                }
            }
            position = SimVector2.Zero;
            return false;
        }

        private List<SimulationEntity> Enemies(SimulationWorld world)
        {
            List<SimulationEntity> result = new List<SimulationEntity>();
            for (int index = 0; index < world.Entities.Count; index++)
            {
                SimulationEntity entity = world.Entities[index];
                if (!entity.IsDestroyed && entity.OwnerUserId != 0 && entity.OwnerUserId != botUserId)
                    result.Add(entity);
            }
            return result;
        }

        private static SimulationEntity NearestInRange(SimVector2 source,
            IReadOnlyList<SimulationEntity> enemies, Fix64 range)
        {
            SimulationEntity nearest = null;
            Fix64 best = Fix64.MaxValue;
            Fix64 limit = range == Fix64.MaxValue ? Fix64.MaxValue : range * range;
            for (int index = 0; index < enemies.Count; index++)
            {
                SimulationEntity enemy = enemies[index];
                Fix64 dx = enemy.Position.X - source.X;
                Fix64 dy = enemy.Position.Y - source.Y;
                Fix64 distance = dx * dx + dy * dy;
                if (distance > limit || distance > best ||
                    distance == best && nearest != null && enemy.Id > nearest.Id) continue;
                nearest = enemy;
                best = distance;
            }
            return nearest;
        }

        private int Cost(string card) => cardCosts.TryGetValue(card, out int value) ? value : 0;
        private Fix64 Range(string card) => cardRanges.TryGetValue(card, out Fix64 value) ? value : (Fix64)10;

        private PracticeBotRecipe SelectCandidate(List<PracticeBotRecipe> candidates,
            SimVector2 origin, IReadOnlyList<SimulationEntity> enemies, int mana)
        {
            candidates.Sort((left, right) =>
            {
                int score = Score(right, origin, enemies, mana).CompareTo(Score(left, origin, enemies, mana));
                return score != 0 ? score : string.CompareOrdinal(RecipeKey(left.Cards), RecipeKey(right.Cards));
            });
            int window = Math.Min(candidates.Count, tierCandidateWindow);
            return candidates[window == 1 ? 0 : random.NextInt(window)];
        }

        private long Score(PracticeBotRecipe recipe, SimVector2 origin,
            IReadOnlyList<SimulationEntity> enemies, int mana)
        {
            long score = recipe.Cards.Count * 10000L + (recipe.Offensive ? 1000 : 500) + mana - recipe.Cost;
            SimulationEntity target = NearestInRange(origin, enemies, recipe.Range);
            return score + (long)CounterScore(recipe.Cards, target) * counterAggressionPermille;
        }

        private static int CounterScore(IReadOnlyList<string> cards, SimulationEntity target)
        {
            if (target == null || string.IsNullOrEmpty(target.PrefabId)) return 0;
            string prefab = target.PrefabId;
            int score = 0;
            for (int index = 0; index < cards.Count; index++)
            {
                string card = cards[index];
                if (Matches(card, "Fire") && (Has(prefab, "Nature") || Has(prefab, "Vine")) ||
                    Matches(card, "Nature") && Has(prefab, "Water") ||
                    Matches(card, "Water") && (Has(prefab, "Fire") || Has(prefab, "Rock")) ||
                    Matches(card, "Lightning") && Has(prefab, "Water") ||
                    Matches(card, "Rock") && Has(prefab, "Lightning") ||
                    Matches(card, "Wind") && Has(prefab, "Nature"))
                    score++;
            }
            return score;
        }

        private static bool Matches(string value, string expected) =>
            string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

        private static bool Has(string value, string token) =>
            value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;

        private static int CandidateWindow(string tier)
        {
            switch (tier?.ToUpperInvariant())
            {
                case "INTRO": return int.MaxValue;
                case "BEGINNER": return 4;
                case "INTERMEDIATE": return 3;
                case "ADVANCED": return 2;
                case "ELITE": return 1;
                default: throw new ArgumentException("Unsupported bot tier: " + tier, nameof(tier));
            }
        }

        private static int NormalizeAggression(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            return (int)Math.Round(Math.Max(0, Math.Min(1, value)) * 1000,
                MidpointRounding.AwayFromZero);
        }

        private static int CompareRecipes(PracticeBotRecipe left, PracticeBotRecipe right)
        {
            int size = right.Cards.Count.CompareTo(left.Cards.Count);
            return size != 0 ? size : string.CompareOrdinal(RecipeKey(left.Cards), RecipeKey(right.Cards));
        }

        private static IEnumerable<PracticeBotRecipe> ToRecipes(IEnumerable<IEnumerable<string>> recipes)
        {
            foreach (IEnumerable<string> recipe in recipes ?? throw new ArgumentNullException(nameof(recipes)))
            {
                string[] cards = new List<string>(recipe ?? Array.Empty<string>()).ToArray();
                if (cards.Length > 0) yield return new PracticeBotRecipe(cards, (Fix64)10, 0,
                    Contains(cards, "Shoot") || Contains(cards, "Explode"));
            }
        }

        private static IEnumerable<IEnumerable<string>> SingleCardRecipes(IEnumerable<string> cards)
        {
            if (cards == null) yield break;
            foreach (string card in cards) yield return new[] { card };
        }

        private static bool ContainsAll(IEnumerable<string> availableCards, IEnumerable<string> requiredCards)
        {
            List<string> available = new List<string>(availableCards);
            foreach (string required in requiredCards)
                if (!available.Remove(required)) return false;
            return true;
        }

        private static bool Contains(IReadOnlyList<string> cards, string value)
        {
            for (int index = 0; index < cards.Count; index++)
                if (string.Equals(cards[index], value, StringComparison.Ordinal)) return true;
            return false;
        }

        private static string RecipeKey(IEnumerable<string> recipe)
        {
            List<string> ordered = new List<string>(recipe);
            ordered.Sort(StringComparer.Ordinal);
            return string.Join("\u001f", ordered);
        }

        private static Dictionary<string, T> Copy<T>(IReadOnlyDictionary<string, T> source)
        {
            Dictionary<string, T> result = new Dictionary<string, T>(StringComparer.Ordinal);
            if (source != null)
                foreach (KeyValuePair<string, T> pair in source) result.Add(pair.Key, pair.Value);
            return result;
        }

        private static int FloatBits(float value) =>
            BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
    }
}

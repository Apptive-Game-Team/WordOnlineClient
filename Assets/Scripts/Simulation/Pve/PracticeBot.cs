using System;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Protocol;

namespace GameScene.Simulation.Pve
{
    public sealed class PracticeBot
    {
        private readonly long botUserId;
        private readonly int reactionIntervalFrames;
        private readonly string[] cards;
        private readonly DeterministicRandom random;
        private int sequence;

        public PracticeBot(long sessionSeed, long botUserId, int reactionIntervalFrames, params string[] cards)
        {
            if (botUserId >= 0) throw new ArgumentOutOfRangeException(nameof(botUserId), "Practice bot ID must be negative");
            if (reactionIntervalFrames <= 0) throw new ArgumentOutOfRangeException(nameof(reactionIntervalFrames));
            if (cards == null || cards.Length == 0) throw new ArgumentException("Bot cards required", nameof(cards));
            this.botUserId = botUserId; this.reactionIntervalFrames = reactionIntervalFrames; this.cards = (string[])cards.Clone();
            Array.Sort(this.cards, StringComparer.Ordinal); random = new DeterministicRandom(unchecked(sessionSeed ^ botUserId));
        }

        public ConfirmedInputMessage Decide(int frameNumber, SimulationWorld world)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (frameNumber < 0 || frameNumber % reactionIntervalFrames != 0) return null;
            SimulationEntity target = null;
            for (int index = 0; index < world.Entities.Count; index++)
            {
                SimulationEntity entity = world.Entities[index];
                if (!entity.IsDestroyed && entity.OwnerUserId >= 0) { target = entity; break; }
            }
            if (target == null) return null;
            string card = cards[random.NextInt(cards.Length)];
            return new ConfirmedInputMessage { userId = botUserId, input = new FrameInputMessage { sequence = sequence++, type = "useMagic", id = 0, cards = new[] { card }, position = new ProtocolVector3 { x = (float)(decimal)target.Position.X, y = 0, z = (float)(decimal)target.Position.Y } } };
        }
    }
}

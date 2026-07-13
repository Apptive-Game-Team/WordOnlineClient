using System;
using System.Collections.Generic;

namespace GameScene.Simulation.Resources
{
    public sealed class PlayerResourceSnapshot
    {
        private readonly string[] hand;

        public long UserId { get; }
        public int Mana { get; }
        public int Health { get; }
        public IReadOnlyList<string> Hand => hand;

        internal PlayerResourceSnapshot(PlayerResourceState state)
        {
            UserId = state.UserId;
            Mana = state.Mana;
            Health = state.Health;
            hand = new string[state.Hand.Count];
            for (int index = 0; index < hand.Length; index++) hand[index] = state.Hand[index];
        }
    }
}

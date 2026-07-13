using System;

namespace GameScene.Simulation.Core
{
    /// <summary>Xoshiro128** with SplitMix64 seed expansion.</summary>
    public sealed class DeterministicRandom
    {
        private uint state0;
        private uint state1;
        private uint state2;
        private uint state3;

        public ulong DrawCount { get; private set; }

        public DeterministicRandom(long seed)
        {
            ulong state = unchecked((ulong)seed);
            state0 = unchecked((uint)SplitMix64(ref state));
            state1 = unchecked((uint)SplitMix64(ref state));
            state2 = unchecked((uint)SplitMix64(ref state));
            state3 = unchecked((uint)SplitMix64(ref state));

            if ((state0 | state1 | state2 | state3) == 0)
            {
                state0 = 1;
            }
        }

        public uint NextUInt32()
        {
            uint result = unchecked(RotateLeft(unchecked(state1 * 5), 7) * 9);
            uint temporary = unchecked(state1 << 9);

            state2 ^= state0;
            state3 ^= state1;
            state1 ^= state2;
            state0 ^= state3;
            state2 ^= temporary;
            state3 = RotateLeft(state3, 11);
            DrawCount++;
            return result;
        }

        public int NextInt(int exclusiveMax)
        {
            if (exclusiveMax <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            }

            // Rejection sampling avoids modulo bias and fixes draw behavior in the contract.
            uint bound = unchecked((uint)exclusiveMax);
            uint threshold = unchecked((uint)(0 - bound)) % bound;
            while (true)
            {
                uint value = NextUInt32();
                if (value >= threshold)
                {
                    return unchecked((int)(value % bound));
                }
            }
        }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteUInt32(state0);
            writer.WriteUInt32(state1);
            writer.WriteUInt32(state2);
            writer.WriteUInt32(state3);
            writer.WriteUInt64(DrawCount);
        }

        private static ulong SplitMix64(ref ulong state)
        {
            ulong value = unchecked(state += 0x9E3779B97F4A7C15UL);
            value = unchecked((value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL);
            value = unchecked((value ^ (value >> 27)) * 0x94D049BB133111EBUL);
            return value ^ (value >> 31);
        }

        private static uint RotateLeft(uint value, int bits)
        {
            return unchecked((value << bits) | (value >> (32 - bits)));
        }
    }
}

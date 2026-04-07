namespace Simulation.Core
{
    /// <summary>
    /// Deterministic PRNG using xoshiro128** algorithm.
    /// Both clients must create with the same seed to produce identical sequences.
    /// </summary>
    public class SimRandom
    {
        private uint _s0, _s1, _s2, _s3;

        public SimRandom(long seed)
        {
            // SplitMix64 to expand a single seed into 4 state words
            ulong s = (ulong)seed;
            _s0 = (uint)SplitMix(ref s);
            _s1 = (uint)SplitMix(ref s);
            _s2 = (uint)SplitMix(ref s);
            _s3 = (uint)SplitMix(ref s);
            if (_s0 == 0 && _s1 == 0 && _s2 == 0 && _s3 == 0)
                _s0 = 1;
        }

        private static ulong SplitMix(ref ulong state)
        {
            ulong z = (state += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <summary>Returns a random uint.</summary>
        public uint NextUint()
        {
            uint result = RotateLeft(_s1 * 5, 7) * 9;
            uint t = _s1 << 9;
            _s2 ^= _s0;
            _s3 ^= _s1;
            _s1 ^= _s2;
            _s0 ^= _s3;
            _s2 ^= t;
            _s3 = RotateLeft(_s3, 11);
            return result;
        }

        /// <summary>Returns a Fix64 in [0, 1). Deterministic.</summary>
        public Fix64 NextFix()
        {
            // Use upper 16 bits of NextUint mapped to [0, ONE)
            // NextUint() >> 16 gives [0, 65535], which maps directly to Fix64 fractional range
            return new Fix64(NextUint() >> 16);
        }

        /// <summary>Returns an int in [min, max).</summary>
        public int Range(int min, int max)
        {
            if (min >= max) return min;
            return min + (int)(NextUint() % (uint)(max - min));
        }

        /// <summary>Returns a Fix64 in [min, max).</summary>
        public Fix64 Range(Fix64 min, Fix64 max)
        {
            return min + NextFix() * (max - min);
        }

        /// <summary>Returns a random unit vector on the XY plane.</summary>
        public SimVector2 RandomUnitVector2()
        {
            Fix64 angle = NextFix() * Fix64.TwoPi;
            return new SimVector2(SimMath.Cos(angle), SimMath.Sin(angle));
        }

        private static uint RotateLeft(uint x, int k)
        {
            return (x << k) | (x >> (32 - k));
        }
    }
}

namespace Simulation.Core
{
    /// <summary>
    /// Deterministic math using Fix64 fixed-point. No floating-point operations.
    /// </summary>
    public static class SimMath
    {
        public static readonly Fix64 PI = Fix64.Pi;
        public static readonly Fix64 TWO_PI = Fix64.TwoPi;

        // ── Basic ──

        public static Fix64 Abs(Fix64 x) => x.Raw < 0 ? -x : x;
        public static Fix64 Min(Fix64 a, Fix64 b) => a < b ? a : b;
        public static Fix64 Max(Fix64 a, Fix64 b) => a > b ? a : b;
        public static int Min(int a, int b) => a < b ? a : b;
        public static int Max(int a, int b) => a > b ? a : b;
        public static Fix64 Clamp(Fix64 v, Fix64 min, Fix64 max) => v < min ? min : v > max ? max : v;
        public static Fix64 Floor(Fix64 x) => new Fix64(x.Raw & ~(Fix64.ONE - 1));
        public static Fix64 Ceil(Fix64 x)
        {
            long frac = x.Raw & (Fix64.ONE - 1);
            return frac == 0 ? x : new Fix64((x.Raw & ~(Fix64.ONE - 1)) + Fix64.ONE);
        }

        public static Fix64 Lerp(Fix64 a, Fix64 b, Fix64 t) => a + (b - a) * t;

        public static int FloorToInt(Fix64 x) => (int)(x.Raw >> Fix64.SHIFT);

        // ── Sqrt (Newton's method in fixed-point) ──

        public static Fix64 Sqrt(Fix64 x)
        {
            if (x.Raw <= 0) return Fix64.Zero;

            // Initial guess: shift right by half the fractional bits
            long val = x.Raw;
            long guess = val;

            // Find a reasonable starting point
            if (val > Fix64.ONE)
                guess = val >> (Fix64.SHIFT / 2);
            else
                guess = Fix64.ONE; // for values < 1, start at 1

            // Newton iterations: guess = (guess + val * ONE / guess) / 2
            // We need sqrt in Q16 format: if input is Q16, output should be Q16
            // sqrt(x_raw / 2^16) = sqrt(x_raw) / sqrt(2^16) = sqrt(x_raw) / 256
            // Result in Q16 = sqrt(x_raw) / 256 * 2^16 = sqrt(x_raw) * 256
            // Or equivalently: sqrt(x_raw << 16)

            // Use the identity: result_raw = sqrt(x_raw << SHIFT)
            // To avoid overflow, compute iteratively
            long shifted = x.Raw << Fix64.SHIFT;

            // Better initial guess using bit length
            int bits = 0;
            long tmp = shifted;
            while (tmp > 0) { bits++; tmp >>= 1; }
            guess = 1L << (bits / 2);

            // Newton's method: 8 iterations is plenty for 64-bit
            for (int i = 0; i < 8; i++)
            {
                if (guess == 0) return Fix64.Zero;
                long next = (guess + shifted / guess) >> 1;
                if (next == guess) break;
                guess = next;
            }

            return new Fix64(guess);
        }

        // ── Trigonometry (lookup table, 1024 entries per quarter) ──

        private const int SIN_TABLE_SIZE = 1024;
        private static readonly Fix64[] _sinTable;

        static SimMath()
        {
            // Pre-compute sine table for [0, pi/2] using double, then store as Fix64.
            // This happens once at startup and is deterministic (same binary = same table).
            _sinTable = new Fix64[SIN_TABLE_SIZE + 1];
            for (int i = 0; i <= SIN_TABLE_SIZE; i++)
            {
                double angle = (double)i / SIN_TABLE_SIZE * (System.Math.PI / 2.0);
                _sinTable[i] = Fix64.FromDouble(System.Math.Sin(angle));
            }
        }

        public static Fix64 Sin(Fix64 x)
        {
            // Normalize to [0, 2*PI)
            long raw = x.Raw % TWO_PI.Raw;
            if (raw < 0) raw += TWO_PI.Raw;

            // Determine quadrant
            long halfPi = Fix64.HalfPi.Raw;
            long pi = Fix64.Pi.Raw;
            long threeHalfPi = halfPi + pi;

            bool negate = false;
            long index;

            if (raw < halfPi)
            {
                // Q1: sin(x) = table[x]
                index = raw * SIN_TABLE_SIZE / halfPi;
            }
            else if (raw < pi)
            {
                // Q2: sin(x) = table[pi - x]
                index = (pi - raw) * SIN_TABLE_SIZE / halfPi;
            }
            else if (raw < threeHalfPi)
            {
                // Q3: sin(x) = -table[x - pi]
                index = (raw - pi) * SIN_TABLE_SIZE / halfPi;
                negate = true;
            }
            else
            {
                // Q4: sin(x) = -table[2pi - x]
                index = (TWO_PI.Raw - raw) * SIN_TABLE_SIZE / halfPi;
                negate = true;
            }

            if (index < 0) index = 0;
            if (index > SIN_TABLE_SIZE) index = SIN_TABLE_SIZE;

            Fix64 result = _sinTable[(int)index];
            return negate ? -result : result;
        }

        public static Fix64 Cos(Fix64 x) => Sin(x + Fix64.HalfPi);
    }
}

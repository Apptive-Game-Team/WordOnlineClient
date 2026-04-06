using System;

namespace Simulation.Core
{
    /// <summary>
    /// 64-bit fixed-point number (Q47.16) for deterministic cross-platform math.
    /// Internal representation: long with 16 fractional bits.
    /// </summary>
    public readonly struct Fix64 : IEquatable<Fix64>, IComparable<Fix64>
    {
        public const int SHIFT = 16;
        public const long ONE = 1L << SHIFT;       // 65536
        public const long HALF_RAW = ONE / 2;       // 32768

        public readonly long Raw;

        public Fix64(long raw) => Raw = raw;

        // ── Factory ──
        public static Fix64 FromInt(int v) => new Fix64((long)v << SHIFT);
        public static Fix64 FromLong(long v) => new Fix64(v << SHIFT);

        /// <summary>Use ONLY for compile-time constants. Never in hot simulation paths.</summary>
        public static Fix64 FromDouble(double v) => new Fix64((long)(v * ONE));

        // ── Constants ──
        public static readonly Fix64 Zero     = new Fix64(0);
        public static readonly Fix64 One      = FromInt(1);
        public static readonly Fix64 MinusOne = FromInt(-1);
        public static readonly Fix64 Half     = new Fix64(HALF_RAW);
        public static readonly Fix64 Two      = FromInt(2);
        public static readonly Fix64 Pi       = FromDouble(3.14159265358979);
        public static readonly Fix64 TwoPi    = FromDouble(6.28318530717959);
        public static readonly Fix64 HalfPi   = FromDouble(1.57079632679490);
        public static readonly Fix64 Epsilon  = new Fix64(1); // smallest representable positive value

        // ── Arithmetic ──
        public static Fix64 operator +(Fix64 a, Fix64 b) => new Fix64(a.Raw + b.Raw);
        public static Fix64 operator -(Fix64 a, Fix64 b) => new Fix64(a.Raw - b.Raw);
        public static Fix64 operator -(Fix64 a) => new Fix64(-a.Raw);

        public static Fix64 operator *(Fix64 a, Fix64 b)
        {
            return new Fix64((a.Raw * b.Raw) >> SHIFT);
        }

        public static Fix64 operator /(Fix64 a, Fix64 b)
        {
            return new Fix64((a.Raw << SHIFT) / b.Raw);
        }

        public static Fix64 operator %(Fix64 a, Fix64 b) => new Fix64(a.Raw % b.Raw);

        // ── Int multiply shortcut (avoids shift) ──
        public static Fix64 operator *(Fix64 a, int b) => new Fix64(a.Raw * b);
        public static Fix64 operator *(int a, Fix64 b) => new Fix64(a * b.Raw);

        // ── Comparison ──
        public static bool operator ==(Fix64 a, Fix64 b) => a.Raw == b.Raw;
        public static bool operator !=(Fix64 a, Fix64 b) => a.Raw != b.Raw;
        public static bool operator <(Fix64 a, Fix64 b) => a.Raw < b.Raw;
        public static bool operator >(Fix64 a, Fix64 b) => a.Raw > b.Raw;
        public static bool operator <=(Fix64 a, Fix64 b) => a.Raw <= b.Raw;
        public static bool operator >=(Fix64 a, Fix64 b) => a.Raw >= b.Raw;

        // ── Conversion (for display / rendering ONLY) ──
        public int ToInt() => (int)(Raw >> SHIFT);
        public float ToFloat() => (float)Raw / ONE;

        // ── Object overrides ──
        public bool Equals(Fix64 other) => Raw == other.Raw;
        public override bool Equals(object obj) => obj is Fix64 other && Raw == other.Raw;
        public override int GetHashCode() => Raw.GetHashCode();
        public int CompareTo(Fix64 other) => Raw.CompareTo(other.Raw);
        public override string ToString() => ToFloat().ToString("F4");
    }
}

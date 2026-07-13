using System;
using System.Globalization;

namespace GameScene.Simulation.Core
{
    /// <summary>
    /// Signed Q47.16 fixed-point value. Simulation code must construct values from
    /// integer or raw representations; floating-point conversion belongs at IO/render boundaries.
    /// Arithmetic throws on overflow so divergent wrapped state cannot remain hidden.
    /// </summary>
    public readonly struct Fixed64 : IEquatable<Fixed64>, IComparable<Fixed64>
    {
        public const int FractionalBits = 16;
        public const long OneRaw = 1L << FractionalBits;

        public static readonly Fixed64 Zero = FromRaw(0);
        public static readonly Fixed64 One = FromRaw(OneRaw);

        public long Raw { get; }

        private Fixed64(long raw)
        {
            Raw = raw;
        }

        public static Fixed64 FromRaw(long raw)
        {
            return new Fixed64(raw);
        }

        public static Fixed64 FromInt(int value)
        {
            return new Fixed64(checked((long)value << FractionalBits));
        }

        public static Fixed64 FromRatio(int numerator, int denominator)
        {
            if (denominator == 0)
            {
                throw new DivideByZeroException();
            }

            return new Fixed64(checked(((long)numerator << FractionalBits) / denominator));
        }

        public int FloorToInt()
        {
            return checked((int)(Raw >> FractionalBits));
        }

        public static Fixed64 operator +(Fixed64 left, Fixed64 right)
        {
            return FromRaw(checked(left.Raw + right.Raw));
        }

        public static Fixed64 operator -(Fixed64 left, Fixed64 right)
        {
            return FromRaw(checked(left.Raw - right.Raw));
        }

        public static Fixed64 operator -(Fixed64 value)
        {
            return FromRaw(checked(-value.Raw));
        }

        public static Fixed64 operator *(Fixed64 left, Fixed64 right)
        {
            // Split at the binary point so common world-sized values do not overflow
            // before the final shift. The remaining fractional product is bounded by 2^32.
            long leftWhole = left.Raw >> FractionalBits;
            long rightWhole = right.Raw >> FractionalBits;
            long leftFraction = left.Raw - (leftWhole << FractionalBits);
            long rightFraction = right.Raw - (rightWhole << FractionalBits);

            long whole = checked(checked(leftWhole * rightWhole) << FractionalBits);
            long crossA = checked(leftWhole * rightFraction);
            long crossB = checked(rightWhole * leftFraction);
            long fraction = checked((leftFraction * rightFraction) >> FractionalBits);
            return FromRaw(checked(checked(whole + crossA) + checked(crossB + fraction)));
        }

        public static Fixed64 operator /(Fixed64 left, Fixed64 right)
        {
            if (right.Raw == 0)
            {
                throw new DivideByZeroException();
            }

            long quotient = left.Raw / right.Raw;
            long remainder = left.Raw % right.Raw;
            long whole = checked(quotient << FractionalBits);
            long fraction = checked((remainder << FractionalBits) / right.Raw);
            return FromRaw(checked(whole + fraction));
        }

        public static bool operator ==(Fixed64 left, Fixed64 right) => left.Raw == right.Raw;
        public static bool operator !=(Fixed64 left, Fixed64 right) => left.Raw != right.Raw;
        public static bool operator <(Fixed64 left, Fixed64 right) => left.Raw < right.Raw;
        public static bool operator >(Fixed64 left, Fixed64 right) => left.Raw > right.Raw;
        public static bool operator <=(Fixed64 left, Fixed64 right) => left.Raw <= right.Raw;
        public static bool operator >=(Fixed64 left, Fixed64 right) => left.Raw >= right.Raw;

        public bool Equals(Fixed64 other) => Raw == other.Raw;
        public override bool Equals(object obj) => obj is Fixed64 other && Equals(other);
        public override int GetHashCode() => Raw.GetHashCode();
        public int CompareTo(Fixed64 other) => Raw.CompareTo(other.Raw);
        public override string ToString() => Raw.ToString(CultureInfo.InvariantCulture);
    }
}

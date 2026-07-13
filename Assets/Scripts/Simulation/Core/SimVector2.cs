using System;
using FixMath.NET;

namespace GameScene.Simulation.Core
{
    public readonly struct SimVector2 : IEquatable<SimVector2>
    {
        public static readonly SimVector2 Zero = new SimVector2(Fix64.Zero, Fix64.Zero);

        public Fix64 X { get; }
        public Fix64 Y { get; }

        public SimVector2(Fix64 x, Fix64 y)
        {
            X = x;
            Y = y;
        }

        public static SimVector2 operator +(SimVector2 left, SimVector2 right)
        {
            return new SimVector2(left.X + right.X, left.Y + right.Y);
        }

        public static SimVector2 operator *(SimVector2 value, Fix64 scalar)
        {
            return new SimVector2(value.X * scalar, value.Y * scalar);
        }

        public bool Equals(SimVector2 other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is SimVector2 other && Equals(other);
        public override int GetHashCode() => unchecked((X.GetHashCode() * 397) ^ Y.GetHashCode());
    }
}

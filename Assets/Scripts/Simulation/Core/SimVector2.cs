using System;

namespace GameScene.Simulation.Core
{
    public readonly struct SimVector2 : IEquatable<SimVector2>
    {
        public static readonly SimVector2 Zero = new SimVector2(Fixed64.Zero, Fixed64.Zero);

        public Fixed64 X { get; }
        public Fixed64 Y { get; }

        public SimVector2(Fixed64 x, Fixed64 y)
        {
            X = x;
            Y = y;
        }

        public static SimVector2 operator +(SimVector2 left, SimVector2 right)
        {
            return new SimVector2(left.X + right.X, left.Y + right.Y);
        }

        public static SimVector2 operator *(SimVector2 value, Fixed64 scalar)
        {
            return new SimVector2(value.X * scalar, value.Y * scalar);
        }

        public bool Equals(SimVector2 other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is SimVector2 other && Equals(other);
        public override int GetHashCode() => unchecked((X.GetHashCode() * 397) ^ Y.GetHashCode());
    }
}

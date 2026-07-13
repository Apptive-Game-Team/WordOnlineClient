using System;
using FixMath.NET;

namespace GameScene.Simulation.Core
{
    public readonly struct SimVector3 : IEquatable<SimVector3>
    {
        public Fix64 X { get; }
        public Fix64 Y { get; }
        public Fix64 Z { get; }

        public SimVector3(Fix64 x, Fix64 y, Fix64 z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(SimVector3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public override bool Equals(object obj) => obj is SimVector3 other && Equals(other);
        public override int GetHashCode() => unchecked(((X.GetHashCode() * 397) ^ Y.GetHashCode()) * 397 ^ Z.GetHashCode());
    }
}

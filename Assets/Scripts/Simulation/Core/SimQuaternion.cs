using System;
using FixMath.NET;

namespace GameScene.Simulation.Core
{
    public readonly struct SimQuaternion : IEquatable<SimQuaternion>
    {
        public Fix64 X { get; }
        public Fix64 Y { get; }
        public Fix64 Z { get; }
        public Fix64 W { get; }

        public SimQuaternion(Fix64 x, Fix64 y, Fix64 z, Fix64 w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public bool Equals(SimQuaternion other) =>
            X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        public override bool Equals(object obj) => obj is SimQuaternion other && Equals(other);
        public override int GetHashCode() => unchecked((((X.GetHashCode() * 397) ^ Y.GetHashCode()) * 397 ^ Z.GetHashCode()) * 397 ^ W.GetHashCode());
    }
}

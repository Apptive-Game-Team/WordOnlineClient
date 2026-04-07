namespace Simulation.Core
{
    public struct SimVector3
    {
        public Fix64 X, Y, Z;

        public static readonly SimVector3 Zero    = new(0, 0, 0);
        public static readonly SimVector3 Right   = new(1, 0, 0);
        public static readonly SimVector3 Left    = new(-1, 0, 0);
        public static readonly SimVector3 Forward = new(0, 1, 0);
        public static readonly SimVector3 Back    = new(0, -1, 0);
        public static readonly SimVector3 Up      = new(0, 0, 1);
        public static readonly SimVector3 Down    = new(0, 0, -1);

        public SimVector3(Fix64 x, Fix64 y, Fix64 z) { X = x; Y = y; Z = z; }
        public SimVector3(int x, int y, int z) { X = Fix64.FromInt(x); Y = Fix64.FromInt(y); Z = Fix64.FromInt(z); }

        public Fix64 SqrMagnitude => X * X + Y * Y + Z * Z;
        public Fix64 Magnitude => SimMath.Sqrt(SqrMagnitude);

        public SimVector3 Normalized
        {
            get
            {
                Fix64 m = Magnitude;
                return m < Fix64.Epsilon ? Zero : new SimVector3(X / m, Y / m, Z / m);
            }
        }

        public Fix64 Distance(SimVector3 other)
        {
            Fix64 dx = X - other.X, dy = Y - other.Y, dz = Z - other.Z;
            return SimMath.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public Fix64 Distance2D(SimVector3 other)
        {
            Fix64 dx = X - other.X, dy = Y - other.Y;
            return SimMath.Sqrt(dx * dx + dy * dy);
        }

        public Fix64 Dot(SimVector3 other) => X * other.X + Y * other.Y + Z * other.Z;
        public Fix64 Dot(SimVector2 other) => X * other.X + Y * other.Y;

        public SimVector2 ToVector2() => new(X, Y);

        public static SimVector3 Lerp(SimVector3 a, SimVector3 b, Fix64 t)
            => new(SimMath.Lerp(a.X, b.X, t), SimMath.Lerp(a.Y, b.Y, t), SimMath.Lerp(a.Z, b.Z, t));

        public static SimVector3 operator +(SimVector3 a, SimVector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static SimVector3 operator +(SimVector3 a, SimVector2 b) => new(a.X + b.X, a.Y + b.Y, a.Z);
        public static SimVector3 operator -(SimVector3 a, SimVector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static SimVector3 operator *(SimVector3 a, Fix64 s) => new(a.X * s, a.Y * s, a.Z * s);
        public static SimVector3 operator *(Fix64 s, SimVector3 a) => new(a.X * s, a.Y * s, a.Z * s);
        public static SimVector3 operator *(SimVector3 a, int s) => new(a.X * s, a.Y * s, a.Z * s);

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}

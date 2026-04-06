namespace Simulation.Core
{
    public struct SimVector2
    {
        public Fix64 X, Y;

        public static readonly SimVector2 Zero = new(Fix64.Zero, Fix64.Zero);

        public SimVector2(Fix64 x, Fix64 y) { X = x; Y = y; }
        public SimVector2(int x, int y) { X = Fix64.FromInt(x); Y = Fix64.FromInt(y); }
        public SimVector2(SimVector3 v) { X = v.X; Y = v.Y; }

        public Fix64 SqrMagnitude => X * X + Y * Y;
        public Fix64 Magnitude => SimMath.Sqrt(SqrMagnitude);

        public SimVector2 Normalized
        {
            get
            {
                Fix64 m = Magnitude;
                return m < Fix64.Epsilon ? Zero : new SimVector2(X / m, Y / m);
            }
        }

        public Fix64 Distance(SimVector2 other)
        {
            Fix64 dx = X - other.X, dy = Y - other.Y;
            return SimMath.Sqrt(dx * dx + dy * dy);
        }

        public Fix64 Dot(SimVector2 other) => X * other.X + Y * other.Y;

        public SimVector3 ToVector3(Fix64 z) => new(X, Y, z);
        public SimVector3 ToVector3() => new(X, Y, Fix64.Zero);

        public static SimVector2 operator +(SimVector2 a, SimVector2 b) => new(a.X + b.X, a.Y + b.Y);
        public static SimVector2 operator -(SimVector2 a, SimVector2 b) => new(a.X - b.X, a.Y - b.Y);
        public static SimVector2 operator *(SimVector2 a, Fix64 s) => new(a.X * s, a.Y * s);
        public static SimVector2 operator *(Fix64 s, SimVector2 a) => new(a.X * s, a.Y * s);
        public static SimVector2 operator *(SimVector2 a, int s) => new(a.X * s, a.Y * s);

        public override string ToString() => $"({X}, {Y})";
    }
}

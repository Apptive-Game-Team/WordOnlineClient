using System;
using GameScene.Simulation.Core;

namespace GameScene.Simulation.Mob
{
    public readonly struct GridPosition : IEquatable<GridPosition>, IComparable<GridPosition>
    {
        public int X { get; }
        public int Y { get; }

        public GridPosition(int x, int y) { X = x; Y = y; }

        public int CompareTo(GridPosition other)
        {
            int y = Y.CompareTo(other.Y);
            return y != 0 ? y : X.CompareTo(other.X);
        }

        public bool Equals(GridPosition other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => unchecked((X * 397) ^ Y);
        public override string ToString() => X + "," + Y;
    }

    public sealed class NavigationGrid
    {
        private readonly bool[] blocked;

        public int Width { get; }
        public int Height { get; }

        public NavigationGrid(int width, int height, System.Collections.Generic.IEnumerable<GridPosition> blockedCells = null)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            Width = width; Height = height; blocked = new bool[checked(width * height)];
            if (blockedCells == null) return;
            foreach (GridPosition cell in blockedCells)
            {
                if (!Contains(cell)) throw new ArgumentOutOfRangeException(nameof(blockedCells), "Blocked cell outside grid: " + cell);
                blocked[IndexOf(cell)] = true;
            }
        }

        public bool Contains(GridPosition cell) => cell.X >= 0 && cell.X < Width && cell.Y >= 0 && cell.Y < Height;
        public bool IsWalkable(GridPosition cell) => Contains(cell) && !blocked[IndexOf(cell)];
        public int IndexOf(GridPosition cell) => checked(cell.Y * Width + cell.X);
        public GridPosition PositionOf(int index) => new GridPosition(index % Width, index / Width);

        public GridPosition ToGrid(SimVector2 position) => new GridPosition(Floor(position.X.RawValue), Floor(position.Y.RawValue));
        public SimVector2 CellCenter(GridPosition cell) => new SimVector2((FixMath.NET.Fix64)cell.X + (FixMath.NET.Fix64)0.5m, (FixMath.NET.Fix64)cell.Y + (FixMath.NET.Fix64)0.5m);

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteInt32(Width); writer.WriteInt32(Height);
            for (int index = 0; index < blocked.Length; index++) writer.WriteBoolean(blocked[index]);
        }

        private static int Floor(long raw)
        {
            long unit = FixMath.NET.Fix64.One.RawValue;
            long value = raw / unit;
            if (raw < 0 && raw % unit != 0) value--;
            return checked((int)value);
        }
    }
}

using System;
using System.Collections.Generic;

namespace GameScene.Simulation.Mob
{
    public sealed class DeterministicAStar
    {
        // Neighbor order is protocol: North, East, South, West.
        private static readonly GridPosition[] NeighborOffsets =
        {
            new GridPosition(0, 1), new GridPosition(1, 0),
            new GridPosition(0, -1), new GridPosition(-1, 0)
        };

        private readonly NavigationGrid grid;

        public DeterministicAStar(NavigationGrid grid) => this.grid = grid ?? throw new ArgumentNullException(nameof(grid));

        public IReadOnlyList<GridPosition> FindPath(GridPosition start, GridPosition goal)
        {
            if (!grid.IsWalkable(start) || !grid.IsWalkable(goal)) return Array.Empty<GridPosition>();
            int count = checked(grid.Width * grid.Height);
            int[] cost = new int[count]; int[] parent = new int[count]; bool[] closed = new bool[count];
            for (int index = 0; index < count; index++) { cost[index] = int.MaxValue; parent[index] = -1; }
            int startIndex = grid.IndexOf(start); int goalIndex = grid.IndexOf(goal);
            cost[startIndex] = 0;
            List<int> open = new List<int> { startIndex };

            while (open.Count > 0)
            {
                int bestOpenIndex = 0;
                for (int index = 1; index < open.Count; index++)
                    if (Compare(open[index], open[bestOpenIndex], cost, goal) < 0) bestOpenIndex = index;
                int currentIndex = open[bestOpenIndex]; open.RemoveAt(bestOpenIndex);
                if (closed[currentIndex]) continue;
                if (currentIndex == goalIndex) return Reconstruct(parent, currentIndex);
                closed[currentIndex] = true;
                GridPosition current = grid.PositionOf(currentIndex);

                for (int neighborOrder = 0; neighborOrder < NeighborOffsets.Length; neighborOrder++)
                {
                    GridPosition offset = NeighborOffsets[neighborOrder];
                    GridPosition neighbor = new GridPosition(current.X + offset.X, current.Y + offset.Y);
                    if (!grid.IsWalkable(neighbor)) continue;
                    int neighborIndex = grid.IndexOf(neighbor);
                    if (closed[neighborIndex]) continue;
                    int candidate = checked(cost[currentIndex] + 10);
                    if (candidate > cost[neighborIndex]) continue;
                    if (candidate == cost[neighborIndex] && parent[neighborIndex] >= 0 &&
                        grid.PositionOf(parent[neighborIndex]).CompareTo(current) <= 0) continue;
                    cost[neighborIndex] = candidate; parent[neighborIndex] = currentIndex;
                    if (!open.Contains(neighborIndex)) open.Add(neighborIndex);
                }
            }
            return Array.Empty<GridPosition>();
        }

        private int Compare(int leftIndex, int rightIndex, int[] cost, GridPosition goal)
        {
            GridPosition left = grid.PositionOf(leftIndex); GridPosition right = grid.PositionOf(rightIndex);
            int leftH = Heuristic(left, goal); int rightH = Heuristic(right, goal);
            int f = checked(cost[leftIndex] + leftH).CompareTo(checked(cost[rightIndex] + rightH));
            if (f != 0) return f;
            int h = leftH.CompareTo(rightH);
            return h != 0 ? h : left.CompareTo(right);
        }

        private static int Heuristic(GridPosition value, GridPosition goal) =>
            checked((Math.Abs(value.X - goal.X) + Math.Abs(value.Y - goal.Y)) * 10);

        private IReadOnlyList<GridPosition> Reconstruct(int[] parent, int current)
        {
            List<GridPosition> path = new List<GridPosition>();
            while (current >= 0) { path.Add(grid.PositionOf(current)); current = parent[current]; }
            path.Reverse(); return path;
        }
    }
}

using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Simple straight-line pathfinder. Matches server's SimplePathFinder.
    /// </summary>
    public class SimPathFinder
    {
        public static readonly Fix64 REACH_THRESHOLD = Fix64.Half;

        public List<SimVector2> FindPath(SimVector2 start, SimVector2 end)
        {
            var path = new List<SimVector2>();
            Fix64 dx = end.X - start.X;
            Fix64 dy = end.Y - start.Y;
            int stepsX = SimMath.Abs(dx).ToInt();
            int stepsY = SimMath.Abs(dy).ToInt();
            int steps = stepsX > stepsY ? stepsX : stepsY;
            if (steps == 0) steps = 1;

            Fix64 stepsF = Fix64.FromInt(steps);
            for (int i = 0; i <= steps; i++)
            {
                Fix64 t = Fix64.FromInt(i) / stepsF;
                Fix64 x = start.X + dx * t;
                Fix64 y = start.Y + dy * t;
                path.Add(new SimVector2(x, y));
            }
            return path;
        }
    }
}

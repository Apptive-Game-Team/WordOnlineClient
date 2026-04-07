using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Deterministic spatial queries over the simulation world.
    /// All math uses Fix64 — no floating-point.
    /// </summary>
    public class SimPhysics
    {
        private readonly List<SimGameObject> _gameObjects;

        public SimPhysics(List<SimGameObject> gameObjects)
        {
            _gameObjects = gameObjects;
        }

        public List<SimGameObject> OverlapSphereAll(SimGameObject obj, Fix64 radius)
        {
            return OverlapSphereAll(obj.Position, radius);
        }

        public List<SimGameObject> OverlapSphereAll(SimVector3 origin, Fix64 radius)
        {
            var result = new List<SimGameObject>();
            foreach (var other in _gameObjects)
            {
                if (!other.IsActive) continue;
                var collider = other.GetComponent<SimCircleCollider>();
                if (collider == null) continue;

                Fix64 dist = origin.Distance(collider.Position);
                if (dist < collider.Radius + radius)
                    result.Add(other);
            }
            return result;
        }

        public List<SimGameObject> OverlapBoxAll(SimVector3 center, SimVector3 size)
        {
            var result = new List<SimGameObject>();
            Fix64 halfX = size.X / Fix64.Two;
            Fix64 halfY = size.Y / Fix64.Two;
            Fix64 halfZ = size.Z / Fix64.Two;

            Fix64 minX = center.X - halfX, maxX = center.X + halfX;
            Fix64 minY = center.Y - halfY, maxY = center.Y + halfY;
            Fix64 minZ = center.Z - halfZ, maxZ = center.Z + halfZ;

            foreach (var other in _gameObjects)
            {
                if (!other.IsActive) continue;
                var pos = other.Position;
                if (pos.X >= minX && pos.X <= maxX &&
                    pos.Y >= minY && pos.Y <= maxY &&
                    pos.Z >= minZ && pos.Z <= maxZ)
                {
                    result.Add(other);
                }
            }
            return result;
        }

        public SimGameObject Raycast(SimGameObject origin, SimVector2 direction, Fix64 distance)
        {
            SimVector2 originPos = origin.Position.ToVector2();
            SimVector2 dirNorm = direction.Normalized;

            SimGameObject closest = null;
            Fix64 closestDist = distance + Fix64.One;

            foreach (var other in _gameObjects)
            {
                if (other == origin || !other.IsActive) continue;

                SimVector2 toOther = other.Position.ToVector2() - originPos;
                Fix64 projection = toOther.Dot(dirNorm);

                if (projection < Fix64.Zero || projection > distance) continue;

                if (IsColliding(origin, other) && projection < closestDist)
                {
                    closest = other;
                    closestDist = projection;
                }
            }
            return closest;
        }

        public static bool IsColliding(SimGameObject a, SimGameObject b)
        {
            var collidersA = a.GetComponents<SimCollider>();
            var collidersB = b.GetComponents<SimCollider>();
            foreach (var ca in collidersA)
                foreach (var cb in collidersB)
                    if (ca.IsCollidingWith(cb))
                        return true;
            return false;
        }
    }
}

using System;
using FixMath.NET;

namespace GameScene.Simulation.Physics
{
    public sealed class SimulationPhysicsDefinition
    {
        public Fix64 Radius { get; }
        public Fix64 Mass { get; }
        public bool IsTrigger { get; }
        public bool CollisionEnabled { get; }
        public bool IsStatic { get; }

        public SimulationPhysicsDefinition(Fix64 radius, Fix64 mass, bool isTrigger = false,
            bool collisionEnabled = true, bool isStatic = false)
        {
            if (radius <= Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(radius));
            if (mass <= Fix64.Zero) throw new ArgumentOutOfRangeException(nameof(mass));
            Radius = radius;
            Mass = mass;
            IsTrigger = isTrigger;
            CollisionEnabled = collisionEnabled;
            IsStatic = isStatic;
        }
    }
}

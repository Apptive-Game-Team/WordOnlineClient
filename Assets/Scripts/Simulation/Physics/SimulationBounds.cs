using System;
using FixMath.NET;
using GameScene.Simulation.Core;

namespace GameScene.Simulation.Physics
{
    public readonly struct SimulationBounds
    {
        public static readonly SimulationBounds Default = new SimulationBounds(
            new SimVector2((Fix64)(-1000), (Fix64)(-1000)),
            new SimVector2((Fix64)1000, (Fix64)1000));

        public SimVector2 Min { get; }
        public SimVector2 Max { get; }

        public SimulationBounds(SimVector2 min, SimVector2 max)
        {
            if (min.X >= max.X || min.Y >= max.Y)
                throw new ArgumentException("Simulation bounds min must be below max");
            Min = min;
            Max = max;
        }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteFixed64(Min.X);
            writer.WriteFixed64(Min.Y);
            writer.WriteFixed64(Max.X);
            writer.WriteFixed64(Max.Y);
        }
    }
}

using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using FixMath.NET;

namespace GameScene.Simulation.Core
{
    public sealed class SimulationEntity
    {
        private readonly Sphere body;

        public int Id { get; }
        public long OwnerUserId { get; }
        public SimVector2 Position => new SimVector2(body.Position.X, body.Position.Y);
        public SimVector2 Velocity => new SimVector2(body.LinearVelocity.X, body.LinearVelocity.Y);
        public bool IsDestroyed { get; private set; }
        internal Sphere Body => body;

        internal SimulationEntity(int id, long ownerUserId, SimVector2 position)
        {
            Id = id;
            OwnerUserId = ownerUserId;
            body = new Sphere(
                new Vector3(position.X, position.Y, Fix64.Zero),
                (Fix64)0.5m,
                Fix64.One);
            body.LinearDamping = Fix64.Zero;
            body.AngularDamping = Fix64.Zero;
        }

        internal void SetVelocity(SimVector2 velocity)
        {
            body.LinearVelocity = new Vector3(velocity.X, velocity.Y, Fix64.Zero);
        }

        internal void Destroy() => IsDestroyed = true;

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteInt32(Id);
            writer.WriteInt64(OwnerUserId);
            writer.WriteFixed64(body.Position.X);
            writer.WriteFixed64(body.Position.Y);
            writer.WriteFixed64(body.Position.Z);
            writer.WriteFixed64(body.LinearVelocity.X);
            writer.WriteFixed64(body.LinearVelocity.Y);
            writer.WriteFixed64(body.LinearVelocity.Z);
            writer.WriteFixed64(body.AngularVelocity.X);
            writer.WriteFixed64(body.AngularVelocity.Y);
            writer.WriteFixed64(body.AngularVelocity.Z);
            writer.WriteFixed64(body.Orientation.X);
            writer.WriteFixed64(body.Orientation.Y);
            writer.WriteFixed64(body.Orientation.Z);
            writer.WriteFixed64(body.Orientation.W);
            writer.WriteBoolean(IsDestroyed);
        }
    }
}

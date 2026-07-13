using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using FixMath.NET;
using System.Collections.Generic;
using GameScene.Simulation.Objects;

namespace GameScene.Simulation.Core
{
    public sealed class SimulationEntity
    {
        private readonly Sphere body;
        private readonly SimulationComponent[] components;

        public int Id { get; }
        public long OwnerUserId { get; }
        public string PrefabId { get; }
        public int SpawnFrame { get; }
        public int DestroyFrame { get; private set; } = -1;
        public IReadOnlyList<SimulationComponent> Components => components;
        public SimVector2 Position => new SimVector2(body.Position.X, body.Position.Y);
        public SimVector2 Velocity => new SimVector2(body.LinearVelocity.X, body.LinearVelocity.Y);
        public bool IsDestroyed { get; private set; }
        internal Sphere Body => body;

        internal SimulationEntity(
            int id,
            long ownerUserId,
            SimVector2 position,
            SimulationPrefabDefinition prefab,
            int spawnFrame)
        {
            Id = id;
            OwnerUserId = ownerUserId;
            PrefabId = prefab.Id;
            SpawnFrame = spawnFrame;
            components = new SimulationComponent[prefab.ComponentIds.Count];
            for (int index = 0; index < components.Length; index++)
                components[index] = new SimulationComponent(prefab.ComponentIds[index], spawnFrame);
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

        internal void Destroy(int frameNumber)
        {
            if (IsDestroyed) return;
            for (int index = 0; index < components.Length; index++) components[index].Destroy(frameNumber);
            DestroyFrame = frameNumber;
            IsDestroyed = true;
        }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteInt32(Id);
            writer.WriteInt64(OwnerUserId);
            writer.WriteString(PrefabId);
            writer.WriteInt32(SpawnFrame);
            writer.WriteInt32(DestroyFrame);
            writer.WriteInt32(components.Length);
            for (int index = 0; index < components.Length; index++) components[index].WriteState(writer);
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

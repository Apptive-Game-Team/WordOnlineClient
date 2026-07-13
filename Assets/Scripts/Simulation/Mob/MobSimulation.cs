using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Core;

namespace GameScene.Simulation.Mob
{
    public sealed class MobAgentState
    {
        private readonly List<GridPosition> path = new List<GridPosition>();
        internal MobDefinition Definition { get; }
        public int EntityId { get; }
        public int Health { get; internal set; }
        public MobBehaviorState State { get; internal set; }
        public int TargetEntityId { get; internal set; } = -1;
        public int NextDecisionFrame { get; internal set; }
        public int NextAttackFrame { get; internal set; }
        public int PathIndex { get; internal set; }
        public IReadOnlyList<GridPosition> Path => path;

        internal MobAgentState(int entityId, MobDefinition definition)
        { EntityId = entityId; Definition = definition; Health = definition.MaxHealth; }

        internal void SetPath(IReadOnlyList<GridPosition> value)
        { path.Clear(); for (int index = 0; index < value.Count; index++) path.Add(value[index]); PathIndex = path.Count > 1 ? 1 : 0; }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteInt32(EntityId); Definition.WriteState(writer);
            writer.WriteInt32(Health); writer.WriteInt32((int)State);
            writer.WriteInt32(TargetEntityId); writer.WriteInt32(NextDecisionFrame); writer.WriteInt32(NextAttackFrame);
            writer.WriteInt32(PathIndex); writer.WriteInt32(path.Count);
            for (int index = 0; index < path.Count; index++) { writer.WriteInt32(path[index].X); writer.WriteInt32(path[index].Y); }
        }
    }

    public sealed class MobSimulation
    {
        private readonly SimulationWorld world;
        private readonly NavigationGrid grid;
        private readonly DeterministicAStar pathFinder;
        private readonly SortedDictionary<int, MobAgentState> agents = new SortedDictionary<int, MobAgentState>();

        public SimulationWorld World => world;
        public IReadOnlyDictionary<int, MobAgentState> Agents => agents;

        public MobSimulation(SimulationWorld world, NavigationGrid grid)
        { this.world = world ?? throw new ArgumentNullException(nameof(world)); this.grid = grid ?? throw new ArgumentNullException(nameof(grid)); pathFinder = new DeterministicAStar(grid); }

        public MobAgentState Register(int entityId, MobDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            RequireEntity(entityId);
            if (agents.ContainsKey(entityId)) throw new InvalidOperationException("Mob already registered: " + entityId);
            MobAgentState state = new MobAgentState(entityId, definition); agents.Add(entityId, state); return state;
        }

        public void Step()
        {
            IReadOnlyList<SimulationInput> inputs = PrepareFrameInputs();
            world.Step(inputs);
            ResolveAfterPhysics();
        }

        public IReadOnlyList<SimulationInput> PrepareFrameInputs()
        {
            List<SimulationInput> inputs = new List<SimulationInput>(agents.Count);
            foreach (KeyValuePair<int, MobAgentState> pair in agents)
            {
                MobAgentState agent = pair.Value;
                SimulationEntity entity = RequireEntity(agent.EntityId);
                if (agent.State == MobBehaviorState.Dead || entity.IsDestroyed) continue;
                UpdateDecision(agent, entity);
                SimVector2 velocity = agent.State == MobBehaviorState.Chasing ? ChaseVelocity(agent, entity) : SimVector2.Zero;
                inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id), SimulationInputType.SetVelocity, entity.Id, velocity));
            }
            return inputs;
        }

        public void ResolveAfterPhysics()
        {
            foreach (KeyValuePair<int, MobAgentState> pair in agents)
            {
                MobAgentState agent = pair.Value;
                if (agent.State != MobBehaviorState.Attacking || world.FrameNumber < agent.NextAttackFrame) continue;
                if (!TryGetLivingTarget(agent.TargetEntityId, out MobAgentState target) || !InRange(agent.EntityId, target.EntityId, agent.Definition.AttackRange))
                { agent.State = MobBehaviorState.Chasing; agent.NextDecisionFrame = world.FrameNumber; continue; }
                target.Health = Math.Max(0, target.Health - agent.Definition.Damage);
                agent.NextAttackFrame = checked(world.FrameNumber + agent.Definition.AttackCooldownFrames);
                if (target.Health == 0)
                { target.State = MobBehaviorState.Dead; target.TargetEntityId = -1; target.SetPath(Array.Empty<GridPosition>()); world.Destroy(target.EntityId); }
            }
        }

        public ulong CalculateStateHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter(); writer.WriteUInt64(world.CalculateStateHash()); grid.WriteState(writer);
            writer.WriteInt32(agents.Count); foreach (MobAgentState agent in agents.Values) agent.WriteState(writer); return writer.Hash;
        }

        private void UpdateDecision(MobAgentState agent, SimulationEntity entity)
        {
            if (agent.TargetEntityId >= 0 && (!TryGetLivingTarget(agent.TargetEntityId, out MobAgentState current) || current.EntityId == agent.EntityId)) ClearTarget(agent);
            if (agent.State == MobBehaviorState.Attacking && agent.TargetEntityId >= 0 && !InRange(agent.EntityId, agent.TargetEntityId, agent.Definition.AttackRange))
            { agent.State = MobBehaviorState.Chasing; agent.NextDecisionFrame = world.FrameNumber; }
            if (world.FrameNumber < agent.NextDecisionFrame && agent.TargetEntityId >= 0) return;

            MobAgentState target = SelectTarget(agent, entity);
            agent.NextDecisionFrame = checked(world.FrameNumber + agent.Definition.RepathIntervalFrames);
            if (target == null) { ClearTarget(agent); return; }
            agent.TargetEntityId = target.EntityId;
            if (InRange(agent.EntityId, target.EntityId, agent.Definition.AttackRange))
            { agent.State = MobBehaviorState.Attacking; agent.SetPath(Array.Empty<GridPosition>()); return; }
            IReadOnlyList<GridPosition> path = pathFinder.FindPath(grid.ToGrid(entity.Position), grid.ToGrid(world.Entities[target.EntityId].Position));
            agent.SetPath(path);
            agent.State = path.Count > 1 ? MobBehaviorState.Chasing : MobBehaviorState.Idle;
        }

        private MobAgentState SelectTarget(MobAgentState self, SimulationEntity selfEntity)
        {
            MobAgentState selected = null; Fix64 selectedDistance = Fix64.MaxValue;
            Fix64 limit = self.Definition.DetectionRange * self.Definition.DetectionRange;
            foreach (KeyValuePair<int, MobAgentState> pair in agents)
            {
                MobAgentState candidate = pair.Value; SimulationEntity entity = world.Entities[candidate.EntityId];
                if (candidate.EntityId == self.EntityId || candidate.State == MobBehaviorState.Dead || entity.IsDestroyed || entity.OwnerUserId == selfEntity.OwnerUserId) continue;
                Fix64 distance = DistanceSquared(selfEntity.Position, entity.Position);
                if (distance > limit || distance > selectedDistance) continue;
                if (distance == selectedDistance && selected != null && candidate.EntityId > selected.EntityId) continue;
                selected = candidate; selectedDistance = distance;
            }
            return selected;
        }

        private SimVector2 ChaseVelocity(MobAgentState agent, SimulationEntity entity)
        {
            while (agent.PathIndex < agent.Path.Count && grid.ToGrid(entity.Position).Equals(agent.Path[agent.PathIndex])) agent.PathIndex++;
            if (agent.PathIndex >= agent.Path.Count) return SimVector2.Zero;
            SimVector2 destination = grid.CellCenter(agent.Path[agent.PathIndex]);
            Fix64 x = destination.X - entity.Position.X; Fix64 y = destination.Y - entity.Position.Y;
            Fix64 length = Fix64.Sqrt(x * x + y * y);
            return length == Fix64.Zero ? SimVector2.Zero : new SimVector2(x / length * agent.Definition.Speed, y / length * agent.Definition.Speed);
        }

        private bool InRange(int leftId, int rightId, Fix64 range) => DistanceSquared(world.Entities[leftId].Position, world.Entities[rightId].Position) <= range * range;
        private static Fix64 DistanceSquared(SimVector2 left, SimVector2 right)
        { Fix64 x = left.X - right.X; Fix64 y = left.Y - right.Y; return x * x + y * y; }
        private void ClearTarget(MobAgentState agent)
        { agent.TargetEntityId = -1; agent.State = MobBehaviorState.Idle; agent.SetPath(Array.Empty<GridPosition>()); }
        private bool TryGetLivingTarget(int entityId, out MobAgentState target) => agents.TryGetValue(entityId, out target) && target.State != MobBehaviorState.Dead && !world.Entities[entityId].IsDestroyed;
        private SimulationEntity RequireEntity(int entityId)
        {
            if (entityId < 0 || entityId >= world.Entities.Count || world.Entities[entityId].Id != entityId) throw new InvalidOperationException("Missing mob entity: " + entityId);
            return world.Entities[entityId];
        }
    }
}

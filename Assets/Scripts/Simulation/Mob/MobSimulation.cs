using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Magic;
using GameScene.Simulation.Objects;

namespace GameScene.Simulation.Mob
{
    public readonly struct MobStatusEvent
    {
        public int TargetEntityId { get; }
        public string StatusId { get; }
        public MobStatusEvent(int targetEntityId, string statusId)
        { TargetEntityId = targetEntityId; StatusId = statusId; }
    }

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
        public SimVector2 LastPathSpawnPosition { get; internal set; }
        public int NextSelfHealFrame { get; internal set; }
        public int NextAreaEffectFrame { get; internal set; }
        public int ExpireFrame { get; internal set; }
        public int NextPeriodicSpawnFrame { get; internal set; }
        public int PeriodicSpawnCount { get; internal set; }
        public int PanicUntilFrame { get; internal set; }
        public int PanicCooldownUntilFrame { get; internal set; }
        public SimVector2 PanicDirection { get; internal set; }
        public SimVector2 DiveStartPosition { get; internal set; }
        public Fix64 DiveStartHeight { get; internal set; }
        public Fix64 VerticalVelocity { get; internal set; }
        public int BubbleShieldFrames { get; internal set; }
        public int NextSecondaryAttackFrame { get; internal set; }
        public string SelectedPeriodicSpawnPrefabId { get; internal set; }
        public int NextBossMagicFrame { get; internal set; }
        public int BossCastSequence { get; internal set; }
        public int RageCastsRemaining { get; internal set; }
        public int NextRageCastFrame { get; internal set; }
        public bool RageActivated { get; internal set; }
        public int SnaredFrames { get; internal set; }
        public int SlowFrames { get; internal set; }
        public int StunnedFrames { get; internal set; }
        public int ShockOverloadFrames { get; internal set; }
        public int WetFrames { get; internal set; }
        public int FrenzyFrames { get; internal set; }
        public int InspiredFrames { get; internal set; }
        public int SnaredRevision { get; internal set; }
        public int FrenzyRevision { get; internal set; }
        public int InspiredRevision { get; internal set; }
        public long FrenzyOriginalOwnerUserId { get; internal set; }
        public int ActivationFrame { get; internal set; }
        public int AttachedEntityId { get; internal set; } = -1;
        public int RallySourceEntityId { get; internal set; } = -1;
        public int NextAuraFrame { get; internal set; }
        public IReadOnlyList<GridPosition> Path => path;

        internal MobAgentState(int entityId, MobDefinition definition, SimVector2 position, int frameNumber)
        {
            EntityId = entityId; Definition = definition; Health = definition.MaxHealth;
            ActivationFrame = checked(frameNumber + definition.SpawnDelayFrames);
            LastPathSpawnPosition = position;
            NextSelfHealFrame = definition.SelfHealIntervalFrames == 0
                ? 0 : checked(frameNumber + definition.SelfHealIntervalFrames);
            NextAreaEffectFrame = definition.AreaEffectIntervalFrames == 0
                ? 0 : checked(frameNumber + definition.AreaEffectIntervalFrames);
            ExpireFrame = definition.LifetimeFrames == 0
                ? 0 : checked(frameNumber + definition.LifetimeFrames);
            if (definition.Archetype == MobArchetype.Turret ||
                definition.Archetype == MobArchetype.Tower ||
                definition.Archetype == MobArchetype.LightningTower ||
                definition.Archetype == MobArchetype.StationarySummoner)
                NextAttackFrame = checked(frameNumber + definition.AttackCooldownFrames - 1);
            if (definition.Archetype == MobArchetype.ThunderBird)
                State = MobBehaviorState.Floating;
            SelectedPeriodicSpawnPrefabId = definition.PeriodicSpawnPrefabId;
            if (definition.PrimaryBossMagicName != null)
                NextBossMagicFrame = checked(frameNumber + definition.AttackCooldownFrames);
            NextPeriodicSpawnFrame = definition.PeriodicSpawnIntervalFrames == 0
                ? 0 : checked(frameNumber + definition.PeriodicSpawnIntervalFrames);
            NextAuraFrame = definition.AuraIntervalFrames == 0
                ? 0 : checked(frameNumber + definition.AuraIntervalFrames);
        }

        internal void SetPath(IReadOnlyList<GridPosition> value)
        { path.Clear(); for (int index = 0; index < value.Count; index++) path.Add(value[index]); PathIndex = path.Count > 1 ? 1 : 0; }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteInt32(EntityId); Definition.WriteState(writer);
            writer.WriteInt32(Health); writer.WriteInt32((int)State);
            writer.WriteInt32(TargetEntityId); writer.WriteInt32(NextDecisionFrame); writer.WriteInt32(NextAttackFrame);
            writer.WriteInt32(PathIndex); writer.WriteInt32(path.Count);
            for (int index = 0; index < path.Count; index++) { writer.WriteInt32(path[index].X); writer.WriteInt32(path[index].Y); }
            writer.WriteFixed64(LastPathSpawnPosition.X); writer.WriteFixed64(LastPathSpawnPosition.Y);
            writer.WriteInt32(NextSelfHealFrame);
            writer.WriteInt32(NextAreaEffectFrame); writer.WriteInt32(ExpireFrame);
            writer.WriteInt32(NextPeriodicSpawnFrame); writer.WriteInt32(PeriodicSpawnCount);
            writer.WriteInt32(PanicUntilFrame); writer.WriteInt32(PanicCooldownUntilFrame);
            writer.WriteFixed64(PanicDirection.X); writer.WriteFixed64(PanicDirection.Y);
            writer.WriteFixed64(DiveStartPosition.X); writer.WriteFixed64(DiveStartPosition.Y);
            writer.WriteFixed64(DiveStartHeight); writer.WriteFixed64(VerticalVelocity);
            writer.WriteInt32(BubbleShieldFrames);
            writer.WriteInt32(NextSecondaryAttackFrame);
            writer.WriteString(SelectedPeriodicSpawnPrefabId);
            writer.WriteInt32(NextBossMagicFrame); writer.WriteInt32(BossCastSequence);
            writer.WriteInt32(RageCastsRemaining); writer.WriteInt32(NextRageCastFrame);
            writer.WriteBoolean(RageActivated);
            writer.WriteInt32(SnaredFrames); writer.WriteInt32(SlowFrames);
            writer.WriteInt32(StunnedFrames);
            writer.WriteInt32(ShockOverloadFrames); writer.WriteInt32(WetFrames);
            writer.WriteInt32(FrenzyFrames); writer.WriteInt32(InspiredFrames);
            writer.WriteInt32(SnaredRevision); writer.WriteInt32(FrenzyRevision);
            writer.WriteInt32(InspiredRevision);
            writer.WriteInt64(FrenzyOriginalOwnerUserId);
            writer.WriteInt32(ActivationFrame);
            writer.WriteInt32(AttachedEntityId);
            writer.WriteInt32(RallySourceEntityId);
            writer.WriteInt32(NextAuraFrame);
        }
    }

    public sealed class MobSimulation
    {
        private sealed class PendingDamage
        {
            public int ApplyFrame;
            public int SourceEntityId;
            public int TargetEntityId;
            public int Amount;
            public string StatusId;
        }

        private readonly SimulationWorld world;
        private readonly NavigationGrid grid;
        private readonly DeterministicAStar pathFinder;
        private readonly SortedDictionary<int, MobAgentState> agents = new SortedDictionary<int, MobAgentState>();
        private readonly List<PendingDamage> pendingDamage = new List<PendingDamage>();
        private readonly List<MagicCastCommand> generatedMagicCasts = new List<MagicCastCommand>();
        private readonly List<MobStatusEvent> generatedStatusEffects = new List<MobStatusEvent>();
        private bool playerSidesConfigured;
        private long leftUserId;
        private long rightUserId;

        public SimulationWorld World => world;
        public IReadOnlyDictionary<int, MobAgentState> Agents => agents;
        public IReadOnlyList<MagicCastCommand> GeneratedMagicCasts => generatedMagicCasts;
        public IReadOnlyList<MobStatusEvent> GeneratedStatusEffects => generatedStatusEffects;

        public MobSimulation(SimulationWorld world, NavigationGrid grid)
        { this.world = world ?? throw new ArgumentNullException(nameof(world)); this.grid = grid ?? throw new ArgumentNullException(nameof(grid)); pathFinder = new DeterministicAStar(grid); }

        public void ConfigurePlayerSides(long leftUserId, long rightUserId)
        {
            if (leftUserId == rightUserId)
                throw new ArgumentException("Left and right users must be distinct");
            if (playerSidesConfigured && (this.leftUserId != leftUserId || this.rightUserId != rightUserId))
                throw new InvalidOperationException("Mob simulation player sides are already configured");
            this.leftUserId = leftUserId;
            this.rightUserId = rightUserId;
            playerSidesConfigured = true;
        }

        public MobAgentState Register(int entityId, MobDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            RequireEntity(entityId);
            if (agents.ContainsKey(entityId)) throw new InvalidOperationException("Mob already registered: " + entityId);
            SimulationEntity entity = RequireEntity(entityId);
            MobAgentState state = new MobAgentState(entityId, definition, entity.Position, world.FrameNumber);
            if (definition.InitialHeight > Fix64.Zero)
                entity.SetPosition3D(new SimVector3(entity.Position.X, entity.Position.Y,
                    definition.InitialHeight));
            if (definition.SelectSinglePeriodicPrefab)
            {
                string[] choices = definition.TertiaryPeriodicSpawnPrefabId != null
                    ? new[] { definition.PeriodicSpawnPrefabId, definition.SecondaryPeriodicSpawnPrefabId,
                        definition.TertiaryPeriodicSpawnPrefabId }
                    : new[] { definition.PeriodicSpawnPrefabId, definition.SecondaryPeriodicSpawnPrefabId };
                state.SelectedPeriodicSpawnPrefabId = choices[world.DrawRandomUInt32() % (uint)choices.Length];
            }
            agents.Add(entityId, state); return state;
        }

        public void RegisterSpawned(IReadOnlyList<MagicSpawnEvent> spawned, IMobDefinitionResolver resolver)
        {
            if (spawned == null) throw new ArgumentNullException(nameof(spawned));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            for (int index = 0; index < spawned.Count; index++)
            {
                MagicSpawnEvent value = spawned[index];
                if (agents.ContainsKey(value.EntityId)) continue;
                if (resolver.TryResolve(value.Definition.PrefabId, out MobDefinition definition))
                    Register(value.EntityId, definition);
            }
        }

        public void RegisterWorldEntities(IMobDefinitionResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            for (int entityId = 0; entityId < world.Entities.Count; entityId++)
            {
                SimulationEntity entity = world.Entities[entityId];
                if (entity.IsDestroyed || agents.ContainsKey(entityId)) continue;
                if (resolver.TryResolve(entity.PrefabId, out MobDefinition definition))
                    Register(entityId, definition);
            }
        }

        public void ApplyDamage(int entityId, int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (!agents.TryGetValue(entityId, out MobAgentState target))
                throw new InvalidOperationException("Missing mob damage target: " + entityId);
            if (target.State == MobBehaviorState.Dead || world.Entities[entityId].IsDestroyed) return;
            if (amount > 0 && target.BubbleShieldFrames > 0)
            {
                target.BubbleShieldFrames = 0;
                return;
            }
            target.Health = Math.Max(0, target.Health - amount);
            if (target.Definition.RageBossMagicName != null && !target.RageActivated &&
                target.Health > 0 && target.Health <= target.Definition.MaxHealth / 2)
            {
                target.RageActivated = true;
                target.RageCastsRemaining = 4;
                target.NextRageCastFrame = world.FrameNumber;
            }
            if (target.Health == 0) Kill(target);
        }

        private void ApplyAttackDamage(int sourceEntityId, int targetEntityId, int amount)
        {
            if (!agents.TryGetValue(targetEntityId, out MobAgentState target))
                throw new InvalidOperationException("Missing mob damage target: " + targetEntityId);
            SimulationElement defense = world.Entities[targetEntityId].Elements;
            if (target.WetFrames > 0) defense |= SimulationElement.Water;
            int remaining = SimulationElementalChart.ApplyToHealth(
                target.Health, amount, world.Entities[sourceEntityId].Elements, defense);
            int scaled = target.Health - remaining;
            ApplyDamage(targetEntityId, scaled);
        }

        public void ResolveHealth(int entityId, int health)
        {
            if (!agents.TryGetValue(entityId, out MobAgentState target))
                throw new InvalidOperationException("Missing mob health target: " + entityId);
            int value = Math.Max(0, Math.Min(target.Definition.MaxHealth, health));
            if (value < target.Health) ApplyDamage(entityId, target.Health - value);
            else if (target.State != MobBehaviorState.Dead) target.Health = value;
        }

        public Fix64 GetManaChargePercent(long ownerUserId)
        {
            Fix64 total = Fix64.Zero;
            foreach (MobAgentState agent in agents.Values)
                if (agent.State != MobBehaviorState.Dead && !world.Entities[agent.EntityId].IsDestroyed &&
                    world.Entities[agent.EntityId].OwnerUserId == ownerUserId)
                    total += agent.Definition.ManaChargePercent;
            return total;
        }

        public void ResolveBubbleShield(int entityId, int frames)
        {
            if (!agents.TryGetValue(entityId, out MobAgentState target)) return;
            target.BubbleShieldFrames = Math.Max(0, frames);
        }

        public void ResolveStatuses(int entityId, IReadOnlyDictionary<string, int> statuses,
            IReadOnlyDictionary<string, int> revisions = null)
        {
            if (!agents.TryGetValue(entityId, out MobAgentState target)) return;
            target.SnaredFrames = StatusFrames(statuses, "snared");
            target.SnaredRevision = StatusFrames(revisions, "snared");
            target.SlowFrames = StatusFrames(statuses, "slow");
            target.StunnedFrames = Math.Max(StatusFrames(statuses, "stun"),
                Math.Max(StatusFrames(statuses, "shock"), Math.Max(
                    StatusFrames(statuses, "shock-overload"), StatusFrames(statuses, "knockback"))));
            target.ShockOverloadFrames = StatusFrames(statuses, "shock-overload");
            target.WetFrames = StatusFrames(statuses, "wet");
            target.InspiredFrames = StatusFrames(statuses, "inspired");
            target.InspiredRevision = StatusFrames(revisions, "inspired");
            int frenzy = StatusFrames(statuses, "frenzy");
            SimulationEntity entity = world.Entities[entityId];
            bool frenzyControllable = IsFrenzyControllable(target);
            if (frenzy > 0 && target.FrenzyFrames == 0 && frenzyControllable)
            {
                target.FrenzyOriginalOwnerUserId = entity.OwnerUserId;
                entity.SetOwner(0);
            }
            else if (frenzy == 0 && target.FrenzyFrames > 0 &&
                target.FrenzyOriginalOwnerUserId != 0 && entity.OwnerUserId == 0)
            {
                entity.SetOwner(target.FrenzyOriginalOwnerUserId);
                target.FrenzyOriginalOwnerUserId = 0;
            }
            target.FrenzyFrames = frenzyControllable ? frenzy : 0;
            target.FrenzyRevision = target.FrenzyFrames > 0
                ? StatusFrames(revisions, "frenzy") : 0;
        }

        public void ResolveRally(int entityId, int sourceEntityId)
        {
            if (agents.TryGetValue(entityId, out MobAgentState target) &&
                world.Entities[entityId].HasComponent(SimulationPrefabRegistry.BehaviorMobComponentId))
                target.RallySourceEntityId = sourceEntityId;
        }

        public void ResolveLifetimeRecovery(int entityId, bool recover)
        {
            if (!recover || !agents.TryGetValue(entityId, out MobAgentState target) ||
                target.Definition.LifetimeFrames <= 0) return;
            target.ExpireFrame = checked(world.FrameNumber + target.Definition.LifetimeFrames);
        }

        public void Step()
        {
            IReadOnlyList<SimulationInput> inputs = PrepareFrameInputs();
            world.Step(inputs);
            ResolveAfterPhysics();
        }

        public IReadOnlyList<SimulationInput> PrepareFrameInputs()
        {
            generatedMagicCasts.Clear();
            generatedStatusEffects.Clear();
            List<SimulationInput> inputs = new List<SimulationInput>(agents.Count);
            foreach (KeyValuePair<int, MobAgentState> pair in agents)
            {
                MobAgentState agent = pair.Value;
                SimulationEntity entity = RequireEntity(agent.EntityId);
                if (agent.State == MobBehaviorState.Dead || entity.IsDestroyed) continue;
                UpdateAttachedItem(agent, entity);
                if (world.FrameNumber < agent.ActivationFrame)
                {
                    inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id),
                        SimulationInputType.SetVelocity, entity.Id, SimVector2.Zero));
                    continue;
                }
                if (TryRallyVelocity(agent, entity, out SimVector2 rallyVelocity))
                {
                    inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id),
                        SimulationInputType.SetVelocity, entity.Id, rallyVelocity));
                    continue;
                }
                if (agent.StunnedFrames > 0)
                {
                    inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id),
                        SimulationInputType.SetVelocity, entity.Id, SimVector2.Zero));
                    continue;
                }
                if (agent.Definition.IsAerial && agent.SnaredFrames == 0 &&
                    agent.Definition.Archetype != MobArchetype.ThunderBird)
                    UpdateAerialHover(entity);
                if (agent.Definition.Archetype == MobArchetype.PveBoss)
                {
                    UpdatePveBoss(agent, entity);
                    inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id),
                        SimulationInputType.SetVelocity, entity.Id, SimVector2.Zero));
                    continue;
                }
                if (agent.Definition.Archetype == MobArchetype.Dummy ||
                    agent.Definition.Archetype == MobArchetype.Totem)
                {
                    inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id),
                        SimulationInputType.SetVelocity, entity.Id, SimVector2.Zero));
                    continue;
                }
                if (agent.Definition.Archetype == MobArchetype.BubbleGenerator)
                {
                    UpdateBubbleGenerator(agent, entity);
                    inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id),
                        SimulationInputType.SetVelocity, entity.Id, SimVector2.Zero));
                    continue;
                }
                if (agent.Definition.Archetype == MobArchetype.DualAttack)
                    UpdateSecondaryAttack(agent, entity);
                if (IsStationaryAttacker(agent.Definition.Archetype))
                {
                    UpdateStationaryAttack(agent, entity);
                    inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id),
                        SimulationInputType.SetVelocity, entity.Id, SimVector2.Zero));
                    continue;
                }
                if (IsCoward(agent.Definition.Archetype))
                {
                    SimVector2 cowardVelocity = UpdateCoward(agent, entity);
                    inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id),
                        SimulationInputType.SetVelocity, entity.Id, cowardVelocity));
                    continue;
                }
                if (agent.Definition.Archetype == MobArchetype.ThunderBird &&
                    (agent.State == MobBehaviorState.Floating || agent.State == MobBehaviorState.Diving))
                {
                    if (agent.State == MobBehaviorState.Floating) UpdateThunderBirdFloat(agent, entity);
                    inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id),
                        SimulationInputType.SetVelocity, entity.Id, SimVector2.Zero));
                    continue;
                }
                if (agent.Definition.Archetype == MobArchetype.KeepDistance)
                {
                    SimVector2 keepDistanceVelocity = UpdateKeepDistance(agent, entity);
                    inputs.Add(new SimulationInput(entity.OwnerUserId,
                        checked(int.MinValue + entity.Id), SimulationInputType.SetVelocity,
                        entity.Id, keepDistanceVelocity));
                    continue;
                }
                UpdateDecision(agent, entity);
                SimVector2 velocity = agent.State == MobBehaviorState.Chasing ? ChaseVelocity(agent, entity) : SimVector2.Zero;
                inputs.Add(new SimulationInput(entity.OwnerUserId, checked(int.MinValue + entity.Id), SimulationInputType.SetVelocity, entity.Id, velocity));
            }
            ApplyWindPush(inputs);
            return inputs;
        }

        private bool TryRallyVelocity(MobAgentState agent, SimulationEntity entity, out SimVector2 velocity)
        {
            velocity = SimVector2.Zero;
            int sourceId = agent.RallySourceEntityId;
            if (sourceId < 0 || sourceId >= world.Entities.Count || world.Entities[sourceId].IsDestroyed)
            {
                agent.RallySourceEntityId = -1;
                return false;
            }
            SimulationEntity source = world.Entities[sourceId];
            Fix64 dx = source.Position.X - entity.Position.X;
            Fix64 dy = source.Position.Y - entity.Position.Y;
            Fix64 distanceSquared = dx * dx + dy * dy;
            if (distanceSquared <= (Fix64)0.64m) return true;
            Fix64 distance = Fix64.Sqrt(distanceSquared);
            velocity = new SimVector2(dx / distance * agent.Definition.Speed,
                dy / distance * agent.Definition.Speed);
            return true;
        }

        private void UpdateAttachedItem(MobAgentState agent, SimulationEntity entity)
        {
            if (entity.PrefabId != "HealingTotem") return;
            if (agent.AttachedEntityId >= 0)
            {
                entity.SetPosition(world.Entities[agent.AttachedEntityId].Position);
                return;
            }
            for (int candidateId = 0; candidateId < world.Entities.Count; candidateId++)
            {
                SimulationEntity candidate = world.Entities[candidateId];
                if (candidate.Id == entity.Id || candidate.IsDestroyed ||
                    candidate.OwnerUserId != 0 && candidate.OwnerUserId != entity.OwnerUserId) continue;
                Fix64 range = entity.Radius + candidate.Radius;
                if (DistanceSquared3D(entity.Position3D, candidate.Position3D) >= range * range) continue;
                agent.AttachedEntityId = candidate.Id;
                entity.SetPosition(candidate.Position);
                return;
            }
        }

        private void ApplyWindPush(List<SimulationInput> inputs)
        {
            foreach (MobAgentState source in agents.Values)
            {
                if (source.State == MobBehaviorState.Dead || source.Definition.PushForce <= Fix64.Zero) continue;
                SimulationEntity sourceEntity = world.Entities[source.EntityId];
                if (sourceEntity.IsDestroyed) continue;
                if (!playerSidesConfigured)
                    throw new InvalidOperationException("Player sides must be configured before wind push simulation");
                Fix64 direction;
                if (sourceEntity.OwnerUserId == leftUserId) direction = Fix64.One;
                else if (sourceEntity.OwnerUserId == rightUserId) direction = -Fix64.One;
                else continue;
                for (int index = 0; index < inputs.Count; index++)
                {
                    SimulationInput input = inputs[index];
                    if (input.EntityId == source.EntityId || !agents.ContainsKey(input.EntityId)) continue;
                    SimulationEntity target = world.Entities[input.EntityId];
                    if (!target.HasComponent(SimulationPrefabRegistry.RigidBodyComponentId)) continue;
                    Fix64 forward = (target.Position.X - sourceEntity.Position.X) * direction;
                    Fix64 lateral = Fix64.Abs(target.Position.Y - sourceEntity.Position.Y);
                    Fix64 vertical = Fix64.Abs(target.Position3D.Z - sourceEntity.Position3D.Z);
                    if (forward < Fix64.Zero || forward > source.Definition.PushRangeX ||
                        lateral > (Fix64)0.5m ||
                        vertical > source.Definition.PushRangeY / (Fix64)2) continue;
                    inputs[index] = new SimulationInput(input.UserId, input.Sequence, input.Type,
                        input.EntityId, new SimVector2(
                            input.Value.X + direction * source.Definition.PushForce, input.Value.Y));
                }
            }
        }

        public void ResolveAfterPhysics()
        {
            ResolvePendingDamage();
            ResolvePassiveComponents();
            ResolveVerticalPhysics();
            foreach (MobAgentState agent in agents.Values)
                if (agent.Definition.Archetype == MobArchetype.ThunderBird &&
                    agent.State == MobBehaviorState.Diving) ResolveThunderBirdDive(agent);
            foreach (KeyValuePair<int, MobAgentState> pair in agents)
            {
                MobAgentState agent = pair.Value;
                if (agent.StunnedFrames > 0 || agent.State != MobBehaviorState.Attacking ||
                    world.FrameNumber < agent.NextAttackFrame) continue;
                bool stationary = IsStationaryAttacker(agent.Definition.Archetype);
                if (!TryGetLivingTarget(agent.TargetEntityId, out MobAgentState target) ||
                    !(stationary ? InCenterRange(agent.EntityId, target.EntityId, agent.Definition.AttackRange)
                        : InRange(agent.EntityId, target.EntityId, agent.Definition.AttackRange)))
                {
                    agent.State = stationary ? MobBehaviorState.Idle : MobBehaviorState.Chasing;
                    agent.NextDecisionFrame = world.FrameNumber;
                    continue;
                }
                ScheduleAttack(agent, target);
                agent.NextAttackFrame = checked(world.FrameNumber + EffectiveAttackCooldown(agent));
            }
        }

        private void ResolveVerticalPhysics()
        {
            foreach (MobAgentState agent in agents.Values)
            {
                SimulationEntity entity = world.Entities[agent.EntityId];
                if (entity.IsDestroyed || entity.IsStatic ||
                    agent.Definition.Archetype == MobArchetype.ThunderBird ||
                    agent.Definition.IsAerial && agent.SnaredFrames == 0) continue;
                Fix64 height = entity.Position3D.Z;
                if (height <= Fix64.Zero && entity.LinearVelocity.Z <= Fix64.Zero)
                {
                    if (height < Fix64.Zero)
                        entity.SetPosition3D(new SimVector3(entity.Position.X, entity.Position.Y, Fix64.Zero));
                    entity.SetVerticalVelocity(Fix64.Zero);
                    continue;
                }
                Fix64 gravity = agent.Definition.Gravity *
                    (agent.ShockOverloadFrames > 0 ? (Fix64)2 : Fix64.One);
                entity.AddVerticalImpulse(-gravity * SimulationWorld.FixedDeltaTime);
            }
        }

        private void UpdateStationaryAttack(MobAgentState agent, SimulationEntity entity)
        {
            if (world.FrameNumber < agent.NextAttackFrame) return;
            MobAgentState target = SelectTarget(agent, entity);
            if (target == null || !InCenterRange(agent.EntityId, target.EntityId, agent.Definition.AttackRange))
            {
                ClearTarget(agent);
                return;
            }
            agent.TargetEntityId = target.EntityId;
            agent.State = MobBehaviorState.Attacking;
        }

        private void ScheduleAttack(MobAgentState source, MobAgentState centerTarget)
        {
            if (source.Definition.Archetype == MobArchetype.ThunderBird)
            {
                SimulationEntity entity = world.Entities[source.EntityId];
                source.DiveStartPosition = entity.Position;
                source.DiveStartHeight = entity.Position3D.Z;
                source.VerticalVelocity = Fix64.Zero;
                source.State = MobBehaviorState.Diving;
                return;
            }
            if (source.Definition.Archetype == MobArchetype.BubbleSplash)
            {
                ScheduleBubbleSplash(source, centerTarget);
                return;
            }
            if (source.Definition.Archetype == MobArchetype.DoubleTarget)
            {
                ScheduleDoubleTargetAttack(source, centerTarget);
                return;
            }
            if (source.Definition.Archetype == MobArchetype.Spraying)
            {
                ScheduleSprayAttack(source, centerTarget);
                return;
            }
            if (source.Definition.Archetype == MobArchetype.Summoner)
            {
                world.Spawn(source.Definition.SummonPrefabId, 0,
                    world.Entities[centerTarget.EntityId].Position);
                return;
            }
            if (source.Definition.Archetype == MobArchetype.StationarySummoner)
            {
                world.Spawn(source.Definition.SummonPrefabId, world.Entities[source.EntityId].OwnerUserId,
                    world.Entities[centerTarget.EntityId].Position);
                return;
            }
            if (source.Definition.Archetype == MobArchetype.SelfDestruct)
            {
                ExplodeAndExpire(source);
                return;
            }
            if (source.Definition.Archetype == MobArchetype.LightningTower)
            {
                ScheduleChainAttack(source, centerTarget);
                return;
            }
            if (source.Definition.SplashRadius == Fix64.Zero)
            {
                ScheduleDamage(source, centerTarget);
                return;
            }
            SimulationEntity sourceEntity = world.Entities[source.EntityId];
            SimulationEntity centerEntity = world.Entities[centerTarget.EntityId];
            foreach (MobAgentState candidate in agents.Values)
            {
                SimulationEntity candidateEntity = world.Entities[candidate.EntityId];
                if (candidate.State == MobBehaviorState.Dead || candidateEntity.IsDestroyed ||
                    candidateEntity.OwnerUserId == sourceEntity.OwnerUserId) continue;
                Fix64 range = source.Definition.SplashRadius + candidate.Definition.ColliderRadius;
                if (DistanceSquared3D(centerEntity.Position3D, candidateEntity.Position3D) < range * range)
                    ScheduleDamage(source, candidate);
            }
        }

        private void ScheduleDoubleTargetAttack(MobAgentState source, MobAgentState firstTarget)
        {
            ScheduleDamage(source, firstTarget);
            MobAgentState second = null;
            Fix64 selectedDistance = Fix64.MaxValue;
            Fix64 rangeSquared = source.Definition.AttackRange * source.Definition.AttackRange;
            SimulationEntity sourceEntity = world.Entities[source.EntityId];
            foreach (MobAgentState candidate in agents.Values)
            {
                SimulationEntity entity = world.Entities[candidate.EntityId];
                if (candidate.EntityId == firstTarget.EntityId || candidate.State == MobBehaviorState.Dead ||
                    entity.IsDestroyed || entity.OwnerUserId == sourceEntity.OwnerUserId) continue;
                Fix64 distance = DistanceSquared3D(sourceEntity.Position3D, entity.Position3D);
                if (distance > rangeSquared || distance > selectedDistance) continue;
                if (distance == selectedDistance && second != null && candidate.EntityId > second.EntityId) continue;
                second = candidate; selectedDistance = distance;
            }
            ScheduleDamage(source, second ?? firstTarget);
        }

        private void ScheduleSprayAttack(MobAgentState source, MobAgentState target)
        {
            SimVector3 origin = world.Entities[source.EntityId].Position3D;
            SimVector3 destination = world.Entities[target.EntityId].Position3D;
            Fix64 x = destination.X - origin.X;
            Fix64 y = destination.Y - origin.Y;
            Fix64 z = destination.Z - origin.Z;
            Fix64 length = Fix64.Sqrt(x * x + y * y + z * z);
            if (length == Fix64.Zero) return;
            SimVector3 direction = new SimVector3(x / length, y / length, z / length);
            Fix64 radius = source.Definition.AttackRange / (Fix64)2;
            HashSet<int> hit = new HashSet<int>();
            for (int sphere = 0; sphere < 2; sphere++)
            {
                Fix64 distance = radius + sphere * (Fix64)2 * radius;
                SimVector3 center = new SimVector3(
                    origin.X + direction.X * distance, origin.Y + direction.Y * distance,
                    origin.Z + direction.Z * distance);
                foreach (MobAgentState candidate in agents.Values)
                {
                    if (candidate.State == MobBehaviorState.Dead || world.Entities[candidate.EntityId].IsDestroyed ||
                        hit.Contains(candidate.EntityId)) continue;
                    Fix64 overlap = radius + candidate.Definition.ColliderRadius;
                    if (DistanceSquared3D(center, world.Entities[candidate.EntityId].Position3D) < overlap * overlap)
                    {
                        hit.Add(candidate.EntityId);
                        ScheduleDamage(source, candidate);
                    }
                }
            }
        }

        private void ScheduleBubbleSplash(MobAgentState source, MobAgentState centerTarget)
        {
            Fix64 distance = Fix64.Sqrt(DistanceSquared3D(
                world.Entities[source.EntityId].Position3D,
                world.Entities[centerTarget.EntityId].Position3D));
            int delay = Math.Max(1, CeilingToInt(distance / source.Definition.ProjectileSpeed /
                SimulationWorld.FixedDeltaTime));
            SimulationEntity sourceEntity = world.Entities[source.EntityId];
            SimulationEntity center = world.Entities[centerTarget.EntityId];
            foreach (MobAgentState candidate in agents.Values)
            {
                SimulationEntity entity = world.Entities[candidate.EntityId];
                if (candidate.State == MobBehaviorState.Dead || entity.IsDestroyed ||
                    entity.OwnerUserId == sourceEntity.OwnerUserId || !IsAirborne(entity)) continue;
                Fix64 range = Fix64.One + candidate.Definition.ColliderRadius;
                if (DistanceSquared3D(center.Position3D, entity.Position3D) > range * range) continue;
                pendingDamage.Add(new PendingDamage
                {
                    ApplyFrame = checked(world.FrameNumber + delay),
                    SourceEntityId = source.EntityId,
                    TargetEntityId = candidate.EntityId,
                    Amount = source.Definition.Damage
                });
            }
        }

        private void ScheduleChainAttack(MobAgentState source, MobAgentState firstTarget)
        {
            ScheduleDamage(source, firstTarget, source.Definition.Damage);
            HashSet<int> hit = new HashSet<int> { firstTarget.EntityId };
            MobAgentState chainSource = firstTarget;
            for (int chain = 0; chain < source.Definition.ChainCount; chain++)
            {
                MobAgentState next = SelectNearestChainTarget(source, chainSource, hit);
                if (next == null) break;
                ScheduleDamage(source, next, source.Definition.ChainDamage);
                hit.Add(next.EntityId);
                chainSource = next;
            }
        }

        private MobAgentState SelectNearestChainTarget(MobAgentState owner, MobAgentState source,
            HashSet<int> excluded)
        {
            MobAgentState selected = null;
            Fix64 selectedDistance = Fix64.MaxValue;
            Fix64 rangeSquared = owner.Definition.ChainRadius * owner.Definition.ChainRadius;
            long ownerUserId = world.Entities[owner.EntityId].OwnerUserId;
            foreach (MobAgentState candidate in agents.Values)
            {
                SimulationEntity entity = world.Entities[candidate.EntityId];
                if (candidate.State == MobBehaviorState.Dead || entity.IsDestroyed ||
                    entity.OwnerUserId == ownerUserId || excluded.Contains(candidate.EntityId)) continue;
                Fix64 distance = DistanceSquared3D(
                    world.Entities[source.EntityId].Position3D, entity.Position3D);
                if (distance > rangeSquared || distance > selectedDistance) continue;
                if (distance == selectedDistance && selected != null && candidate.EntityId > selected.EntityId) continue;
                selected = candidate;
                selectedDistance = distance;
            }
            return selected;
        }

        private void ScheduleDamage(MobAgentState source, MobAgentState target)
        {
            ScheduleDamage(source, target, source.Definition.Damage);
        }

        private void ScheduleDamage(MobAgentState source, MobAgentState target, int amount)
        {
            if (source.Definition.AttackDelayFrames == 0)
            {
                ApplyAttackDamage(source.EntityId, target.EntityId, amount);
                EmitAttackStatus(source, target.EntityId);
                return;
            }
            pendingDamage.Add(new PendingDamage
            {
                ApplyFrame = checked(world.FrameNumber + source.Definition.AttackDelayFrames),
                SourceEntityId = source.EntityId,
                TargetEntityId = target.EntityId,
                Amount = amount,
                StatusId = source.Definition.AttackStatusId
            });
        }

        private void ResolvePendingDamage()
        {
            pendingDamage.Sort((left, right) =>
            {
                int frame = left.ApplyFrame.CompareTo(right.ApplyFrame);
                if (frame != 0) return frame;
                int source = left.SourceEntityId.CompareTo(right.SourceEntityId);
                return source != 0 ? source : left.TargetEntityId.CompareTo(right.TargetEntityId);
            });
            int resolvedCount = 0;
            while (resolvedCount < pendingDamage.Count)
            {
                PendingDamage value = pendingDamage[resolvedCount];
                if (value.ApplyFrame > world.FrameNumber) break;
                if (TryGetLivingTarget(value.TargetEntityId, out _))
                {
                    ApplyAttackDamage(value.SourceEntityId, value.TargetEntityId, value.Amount);
                    if (!string.IsNullOrEmpty(value.StatusId))
                        generatedStatusEffects.Add(new MobStatusEvent(value.TargetEntityId,
                            value.StatusId));
                }
                resolvedCount++;
            }
            if (resolvedCount > 0) pendingDamage.RemoveRange(0, resolvedCount);
        }

        private void EmitAttackStatus(MobAgentState source, int targetEntityId)
        {
            if (!string.IsNullOrEmpty(source.Definition.AttackStatusId))
                generatedStatusEffects.Add(new MobStatusEvent(targetEntityId,
                    source.Definition.AttackStatusId));
        }

        private void ResolvePassiveComponents()
        {
            foreach (MobAgentState agent in agents.Values)
            {
                if (agent.State == MobBehaviorState.Dead || world.Entities[agent.EntityId].IsDestroyed) continue;
                MobDefinition definition = agent.Definition;
                if (agent.BubbleShieldFrames > 0) agent.BubbleShieldFrames--;
                if (agent.ExpireFrame > 0 && world.FrameNumber >= agent.ExpireFrame)
                {
                    Expire(agent);
                    continue;
                }
                if (definition.SelfHealIntervalFrames > 0 && world.FrameNumber >= agent.NextSelfHealFrame)
                {
                    agent.Health = Math.Min(definition.MaxHealth, agent.Health + definition.SelfHealAmount);
                    agent.NextSelfHealFrame = checked(world.FrameNumber + definition.SelfHealIntervalFrames);
                }
                if (definition.AuraIntervalFrames > 0 && world.FrameNumber >= agent.NextAuraFrame)
                {
                    ApplyAura(agent);
                    agent.NextAuraFrame = checked(world.FrameNumber + definition.AuraIntervalFrames);
                }
                if (definition.AreaEffectIntervalFrames > 0 && world.FrameNumber >= agent.NextAreaEffectFrame)
                {
                    ApplyAreaHealthEffect(agent);
                    agent.NextAreaEffectFrame = checked(world.FrameNumber + definition.AreaEffectIntervalFrames);
                }
                if (definition.PeriodicSpawnPrefabId != null &&
                    (definition.PeriodicSpawnLimit == 0 || agent.PeriodicSpawnCount < definition.PeriodicSpawnLimit) &&
                    world.FrameNumber >= agent.NextPeriodicSpawnFrame)
                {
                    SpawnPeriodicChild(agent);
                    agent.NextPeriodicSpawnFrame = checked(world.FrameNumber + definition.PeriodicSpawnIntervalFrames);
                }
                if (definition.PathSpawnPrefabId == null) continue;
                SimVector2 position = world.Entities[agent.EntityId].Position;
                if (DistanceSquared(position, agent.LastPathSpawnPosition) <
                    definition.PathSpawnDistance * definition.PathSpawnDistance) continue;
                world.Spawn(definition.PathSpawnPrefabId, 0, position);
                agent.LastPathSpawnPosition = position;
            }
        }

        private void ApplyAura(MobAgentState source)
        {
            SimulationEntity center = world.Entities[source.EntityId];
            foreach (MobAgentState targetState in agents.Values)
            {
                SimulationEntity target = world.Entities[targetState.EntityId];
                if (target.IsDestroyed || targetState.State == MobBehaviorState.Dead ||
                    !CanReceiveEffects(target)) continue;
                Fix64 dx = target.Position.X - center.Position.X;
                Fix64 dy = target.Position.Y - center.Position.Y;
                Fix64 dz = target.Position3D.Z - center.Position3D.Z;
                Fix64 overlap = source.Definition.AuraRange + targetState.Definition.ColliderRadius;
                if (dx * dx + dy * dy + dz * dz < overlap * overlap)
                    generatedStatusEffects.Add(new MobStatusEvent(target.Id,
                        source.Definition.AuraStatusId));
            }
        }

        private void SpawnPeriodicChild(MobAgentState source)
        {
            SimVector2 position = world.Entities[source.EntityId].Position;
            string prefabId = source.Definition.SelectSinglePeriodicPrefab
                ? source.SelectedPeriodicSpawnPrefabId
                : source.Definition.SecondaryPeriodicSpawnPrefabId != null && source.PeriodicSpawnCount % 2 == 1
                    ? source.Definition.SecondaryPeriodicSpawnPrefabId
                    : source.Definition.PeriodicSpawnPrefabId;
            if (UsesLimitedSequenceSpawner(world.Entities[source.EntityId].PrefabId))
            {
                int slot = source.PeriodicSpawnCount % 6;
                Fix64 offsetX = (Fix64)((slot % 3) - 1) * (Fix64)0.35m;
                Fix64 offsetHeight = ((Fix64)(slot / 3) - (Fix64)0.5m) * (Fix64)0.35m;
                SimulationEntity owner = world.Entities[source.EntityId];
                world.Spawn(prefabId, owner.OwnerUserId,
                    new SimVector3(position.X + offsetX, position.Y,
                        owner.Position3D.Z + offsetHeight));
                source.PeriodicSpawnCount++;
                return;
            }
            Fix64 center = (Fix64)(source.Definition.PeriodicSpawnBatchCount - 1) / (Fix64)2;
            for (int index = 0; index < source.Definition.PeriodicSpawnBatchCount; index++)
                world.Spawn(prefabId, world.Entities[source.EntityId].OwnerUserId,
                    new SimVector2(position.X + ((Fix64)index - center) * (Fix64)0.5m, position.Y));
            source.PeriodicSpawnCount++;
            if (source.Definition.PeriodicSelfDamage > 0)
                ApplyDamage(source.EntityId, source.Definition.PeriodicSelfDamage);
        }

        private static bool UsesLimitedSequenceSpawner(string prefabId) =>
            prefabId == "FireLordSpirit" || prefabId == "DimensionToad";

        private void ApplyAreaHealthEffect(MobAgentState source)
        {
            Fix64 rangeSquared = source.Definition.AreaEffectRange * source.Definition.AreaEffectRange;
            SimulationEntity sourceEntity = world.Entities[source.EntityId];
            foreach (MobAgentState target in agents.Values)
            {
                if (target.State == MobBehaviorState.Dead || world.Entities[target.EntityId].IsDestroyed) continue;
                SimulationEntity targetEntity = world.Entities[target.EntityId];
                if (targetEntity.OwnerUserId != sourceEntity.OwnerUserId ||
                    DistanceSquared3D(sourceEntity.Position3D, targetEntity.Position3D) >= rangeSquared) continue;
                int delta = source.Definition.AreaHealthDelta;
                if (delta < 0) target.Health = Math.Min(target.Definition.MaxHealth, target.Health - delta);
                else ApplyDamage(target.EntityId, delta);
            }
        }

        private void UpdateBubbleGenerator(MobAgentState source, SimulationEntity sourceEntity)
        {
            if (world.FrameNumber < source.NextAttackFrame) return;
            MobAgentState selected = null;
            Fix64 selectedDistance = Fix64.MaxValue;
            foreach (MobAgentState candidate in agents.Values)
            {
                SimulationEntity entity = world.Entities[candidate.EntityId];
                if (candidate.EntityId == source.EntityId || candidate.State == MobBehaviorState.Dead ||
                    entity.IsDestroyed || entity.OwnerUserId != sourceEntity.OwnerUserId ||
                    candidate.BubbleShieldFrames > 0 || !CanReceiveEffects(entity)) continue;
                Fix64 distance = DistanceSquared3D(sourceEntity.Position3D, entity.Position3D);
                Fix64 overlap = source.Definition.AttackRange + candidate.Definition.ColliderRadius;
                if (distance >= overlap * overlap || distance > selectedDistance) continue;
                if (distance == selectedDistance && selected != null && candidate.EntityId > selected.EntityId) continue;
                selected = candidate;
                selectedDistance = distance;
            }
            if (selected == null) return;
            selected.BubbleShieldFrames = 160;
            source.NextAttackFrame = checked(world.FrameNumber + EffectiveAttackCooldown(source));
        }

        private void UpdatePveBoss(MobAgentState source, SimulationEntity sourceEntity)
        {
            if (source.Definition.PrimaryBossMagicName == null) return;
            MobAgentState target = SelectTarget(source, sourceEntity);
            if (target == null || !InRange(source.EntityId, target.EntityId, source.Definition.AttackRange)) return;
            if (source.RageCastsRemaining > 0)
            {
                if (world.FrameNumber < source.NextRageCastFrame) return;
                GenerateBossCast(source, target, source.Definition.RageBossMagicName);
                source.RageCastsRemaining--;
                source.NextRageCastFrame = checked(world.FrameNumber + 9);
                return;
            }
            if (world.FrameNumber < source.NextBossMagicFrame) return;
            GenerateBossCast(source, target, source.Definition.PrimaryBossMagicName);
            source.NextBossMagicFrame = checked(world.FrameNumber + source.Definition.AttackCooldownFrames);
        }

        private void GenerateBossCast(MobAgentState source, MobAgentState target, string magicName)
        {
            if (magicName == null) return;
            source.BossCastSequence++;
            generatedMagicCasts.Add(new MagicCastCommand(
                world.Entities[source.EntityId].OwnerUserId,
                checked(int.MinValue + source.EntityId * 10000 + source.BossCastSequence),
                magicName, world.Entities[target.EntityId].Position, source.EntityId));
        }

        private void UpdateSecondaryAttack(MobAgentState source, SimulationEntity sourceEntity)
        {
            if (world.FrameNumber < source.NextSecondaryAttackFrame) return;
            MobAgentState selected = null;
            Fix64 selectedDistance = Fix64.MaxValue;
            Fix64 limit = source.Definition.SecondaryAttackRange * source.Definition.SecondaryAttackRange;
            foreach (MobAgentState candidate in agents.Values)
            {
                SimulationEntity entity = world.Entities[candidate.EntityId];
                if (candidate.State == MobBehaviorState.Dead || entity.IsDestroyed ||
                    entity.OwnerUserId == sourceEntity.OwnerUserId || !IsAirborne(entity)) continue;
                Fix64 distance = DistanceSquared3D(sourceEntity.Position3D, entity.Position3D);
                if (distance > limit || distance > selectedDistance) continue;
                if (distance == selectedDistance && selected != null && candidate.EntityId > selected.EntityId) continue;
                selected = candidate;
                selectedDistance = distance;
            }
            if (selected == null) return;
            if (source.Definition.SecondaryAttackDelayFrames == 0)
                ApplyAttackDamage(source.EntityId, selected.EntityId, source.Definition.SecondaryDamage);
            else
                pendingDamage.Add(new PendingDamage
                {
                    ApplyFrame = checked(world.FrameNumber + source.Definition.SecondaryAttackDelayFrames),
                    SourceEntityId = source.EntityId,
                    TargetEntityId = selected.EntityId,
                    Amount = source.Definition.SecondaryDamage
                });
            source.NextSecondaryAttackFrame = checked(
                world.FrameNumber + EffectiveAttackCooldown(
                    source, source.Definition.SecondaryAttackCooldownFrames));
        }

        private void Expire(MobAgentState target)
        {
            target.State = MobBehaviorState.Dead;
            target.TargetEntityId = -1;
            target.SetPath(Array.Empty<GridPosition>());
            world.Destroy(target.EntityId);
        }

        private void Kill(MobAgentState target)
        {
            target.State = MobBehaviorState.Dead;
            target.TargetEntityId = -1;
            target.SetPath(Array.Empty<GridPosition>());
            if (target.Definition.DeathSpawnPrefabId != null)
                world.Spawn(target.Definition.DeathSpawnPrefabId, 0, world.Entities[target.EntityId].Position);
            if (target.Definition.Archetype == MobArchetype.SelfDestruct)
                ApplyExplosionDamage(target);
            world.Destroy(target.EntityId);
        }

        private void ExplodeAndExpire(MobAgentState source)
        {
            if (source.State == MobBehaviorState.Dead || world.Entities[source.EntityId].IsDestroyed) return;
            source.State = MobBehaviorState.Dead;
            source.TargetEntityId = -1;
            source.SetPath(Array.Empty<GridPosition>());
            ApplyExplosionDamage(source);
            world.Destroy(source.EntityId);
        }

        private void ApplyExplosionDamage(MobAgentState source)
        {
            List<int> targets = new List<int>();
            foreach (MobAgentState candidate in agents.Values)
            {
                if (candidate.EntityId == source.EntityId || candidate.State == MobBehaviorState.Dead ||
                    world.Entities[candidate.EntityId].IsDestroyed) continue;
                Fix64 radius = source.Definition.ExplosionRadius + candidate.Definition.ColliderRadius;
                if (DistanceSquared3D(world.Entities[source.EntityId].Position3D,
                    world.Entities[candidate.EntityId].Position3D) < radius * radius)
                    targets.Add(candidate.EntityId);
            }
            for (int index = 0; index < targets.Count; index++)
                ApplyAttackDamage(source.EntityId, targets[index], source.Definition.Damage);
        }

        public ulong CalculateStateHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter(); writer.WriteUInt64(world.CalculateStateHash()); grid.WriteState(writer);
            writer.WriteBoolean(playerSidesConfigured); writer.WriteInt64(leftUserId); writer.WriteInt64(rightUserId);
            writer.WriteInt32(agents.Count); foreach (MobAgentState agent in agents.Values) agent.WriteState(writer);
            pendingDamage.Sort((left, right) =>
            {
                int frame = left.ApplyFrame.CompareTo(right.ApplyFrame);
                if (frame != 0) return frame;
                int source = left.SourceEntityId.CompareTo(right.SourceEntityId);
                return source != 0 ? source : left.TargetEntityId.CompareTo(right.TargetEntityId);
            });
            writer.WriteInt32(pendingDamage.Count);
            for (int index = 0; index < pendingDamage.Count; index++)
            {
                PendingDamage value = pendingDamage[index];
                writer.WriteInt32(value.ApplyFrame); writer.WriteInt32(value.SourceEntityId);
                writer.WriteInt32(value.TargetEntityId); writer.WriteInt32(value.Amount);
                writer.WriteString(value.StatusId);
            }
            writer.WriteInt32(generatedMagicCasts.Count);
            for (int index = 0; index < generatedMagicCasts.Count; index++)
            {
                MagicCastCommand cast = generatedMagicCasts[index];
                writer.WriteInt64(cast.UserId); writer.WriteInt32(cast.RequestId);
                writer.WriteString(cast.MagicName); writer.WriteFixed64(cast.Target.X);
                writer.WriteFixed64(cast.Target.Y); writer.WriteInt32(cast.CasterEntityId);
            }
            writer.WriteInt32(generatedStatusEffects.Count);
            for (int index = 0; index < generatedStatusEffects.Count; index++)
            {
                writer.WriteInt32(generatedStatusEffects[index].TargetEntityId);
                writer.WriteString(generatedStatusEffects[index].StatusId);
            }
            return writer.Hash;
        }

        private void UpdateDecision(MobAgentState agent, SimulationEntity entity)
        {
            if (agent.TargetEntityId >= 0 && (!TryGetLivingTarget(agent.TargetEntityId, out MobAgentState current) || current.EntityId == agent.EntityId)) ClearTarget(agent);
            if (agent.State == MobBehaviorState.Attacking && agent.TargetEntityId >= 0 && !InRange(agent.EntityId, agent.TargetEntityId, agent.Definition.AttackRange))
            { agent.State = MobBehaviorState.Chasing; agent.NextDecisionFrame = world.FrameNumber; }
            if (agent.State == MobBehaviorState.Attacking && agent.TargetEntityId >= 0) return;
            if (world.FrameNumber < agent.NextDecisionFrame && agent.TargetEntityId >= 0) return;

            MobAgentState target = SelectTarget(agent, entity);
            agent.NextDecisionFrame = checked(world.FrameNumber + agent.Definition.RepathIntervalFrames);
            if (target == null) { ClearTarget(agent); return; }
            agent.TargetEntityId = target.EntityId;
            if (InEntryRange(agent.EntityId, target.EntityId, agent.Definition.AttackRange))
            {
                if (agent.Definition.Archetype == MobArchetype.KeepDistance)
                {
                    agent.State = MobBehaviorState.Idle;
                    agent.SetPath(Array.Empty<GridPosition>());
                    return;
                }
                agent.State = MobBehaviorState.Attacking;
                agent.NextAttackFrame = checked(world.FrameNumber + EffectiveAttackCooldown(agent));
                agent.SetPath(Array.Empty<GridPosition>());
                return;
            }
            IReadOnlyList<GridPosition> path = pathFinder.FindPath(grid.ToGrid(entity.Position), grid.ToGrid(world.Entities[target.EntityId].Position));
            agent.SetPath(path);
            agent.State = path.Count > 1 ? MobBehaviorState.Chasing : MobBehaviorState.Idle;
        }

        private MobAgentState SelectTarget(MobAgentState self, SimulationEntity selfEntity)
        {
            MobAgentState selected = null; Fix64 selectedDistance = Fix64.MaxValue;
            bool selectedIsPlayer = false;
            Fix64 limit = self.Definition.DetectionRange * self.Definition.DetectionRange;
            foreach (KeyValuePair<int, MobAgentState> pair in agents)
            {
                MobAgentState candidate = pair.Value; SimulationEntity entity = world.Entities[candidate.EntityId];
                if (candidate.EntityId == self.EntityId || candidate.State == MobBehaviorState.Dead || entity.IsDestroyed || entity.OwnerUserId == selfEntity.OwnerUserId) continue;
                MobTargetLayer candidateLayer = IsAirborne(entity)
                    ? MobTargetLayer.Air : MobTargetLayer.Ground;
                if ((self.Definition.TargetLayer & candidateLayer) == 0) continue;
                Fix64 distance = DistanceSquared3D(selfEntity.Position3D, entity.Position3D);
                if (distance > limit) continue;
                bool player = entity.PrefabId == Objects.SimulationPrefabRegistry.PlayerPrefabId;
                if (self.Definition.Archetype == MobArchetype.PlayerPriorityMelee)
                {
                    if (selected != null && selectedIsPlayer && !player) continue;
                    if (player && !selectedIsPlayer) { selected = null; selectedDistance = Fix64.MaxValue; }
                }
                if (distance > selectedDistance) continue;
                if (distance == selectedDistance && selected != null && candidate.EntityId > selected.EntityId) continue;
                selected = candidate; selectedDistance = distance; selectedIsPlayer = player;
            }
            return selected;
        }

        private SimVector2 UpdateCoward(MobAgentState agent, SimulationEntity entity)
        {
            if (agent.PanicUntilFrame > world.FrameNumber)
                return agent.PanicDirection * (agent.Definition.Speed * agent.Definition.PanicSpeedMultiplier);
            if (agent.PanicUntilFrame != 0)
            {
                agent.PanicUntilFrame = 0;
                agent.PanicCooldownUntilFrame = checked(world.FrameNumber + agent.Definition.PanicCooldownFrames);
                ClearTarget(agent);
            }

            if (world.FrameNumber >= agent.PanicCooldownUntilFrame)
            {
                MobAgentState threat = SelectCowardTarget(agent, entity, false, agent.Definition.DetectionRange);
                if (threat != null)
                {
                    SimVector2 threatPosition = world.Entities[threat.EntityId].Position;
                    Fix64 x = entity.Position.X - threatPosition.X;
                    Fix64 y = entity.Position.Y - threatPosition.Y;
                    Fix64 length = Fix64.Sqrt(x * x + y * y);
                    agent.PanicDirection = length == Fix64.Zero ? new SimVector2(Fix64.One, Fix64.Zero) :
                        new SimVector2(x / length, y / length);
                    agent.PanicUntilFrame = checked(world.FrameNumber + agent.Definition.PanicDurationFrames);
                    agent.TargetEntityId = threat.EntityId;
                    agent.State = MobBehaviorState.Chasing;
                    agent.SetPath(Array.Empty<GridPosition>());
                    return agent.PanicDirection * (agent.Definition.Speed * agent.Definition.PanicSpeedMultiplier);
                }
            }

            MobAgentState objective = SelectCowardTarget(agent, entity, true, Fix64.MaxValue);
            if (objective == null) { ClearTarget(agent); return SimVector2.Zero; }
            agent.TargetEntityId = objective.EntityId;
            if (InRange(agent.EntityId, objective.EntityId, agent.Definition.AttackRange))
            {
                agent.State = MobBehaviorState.Attacking;
                if (agent.NextAttackFrame == 0)
                    agent.NextAttackFrame = checked(world.FrameNumber + EffectiveAttackCooldown(agent));
                return SimVector2.Zero;
            }
            IReadOnlyList<GridPosition> path = pathFinder.FindPath(
                grid.ToGrid(entity.Position), grid.ToGrid(world.Entities[objective.EntityId].Position));
            agent.SetPath(path);
            agent.State = path.Count > 1 ? MobBehaviorState.Chasing : MobBehaviorState.Idle;
            return agent.State == MobBehaviorState.Chasing ? ChaseVelocity(agent, entity) : SimVector2.Zero;
        }

        private SimVector2 UpdateKeepDistance(MobAgentState agent, SimulationEntity entity)
        {
            bool validTarget = agent.TargetEntityId >= 0 &&
                TryGetLivingTarget(agent.TargetEntityId, out MobAgentState current) &&
                current.EntityId != agent.EntityId &&
                world.Entities[current.EntityId].OwnerUserId != entity.OwnerUserId;
            if (!validTarget || world.FrameNumber >= agent.NextDecisionFrame)
            {
                MobAgentState selected = SelectTarget(agent, entity);
                agent.NextDecisionFrame = checked(world.FrameNumber + agent.Definition.RepathIntervalFrames);
                if (selected == null)
                {
                    ClearTarget(agent);
                    return SimVector2.Zero;
                }
                agent.TargetEntityId = selected.EntityId;
                IReadOnlyList<GridPosition> path = pathFinder.FindPath(
                    grid.ToGrid(entity.Position),
                    grid.ToGrid(world.Entities[selected.EntityId].Position));
                agent.SetPath(path);
            }
            if (InRange(agent.EntityId, agent.TargetEntityId,
                agent.Definition.AttackRange + (Fix64)0.25m))
            {
                agent.State = MobBehaviorState.Idle;
                return SimVector2.Zero;
            }
            agent.State = MobBehaviorState.Chasing;
            return ChaseVelocity(agent, entity);
        }

        private MobAgentState SelectCowardTarget(MobAgentState self, SimulationEntity selfEntity,
            bool objective, Fix64 range)
        {
            MobAgentState selected = null;
            Fix64 selectedDistance = Fix64.MaxValue;
            Fix64 limit = range == Fix64.MaxValue ? Fix64.MaxValue : range * range;
            foreach (MobAgentState candidate in agents.Values)
            {
                SimulationEntity entity = world.Entities[candidate.EntityId];
                if (candidate.EntityId == self.EntityId || candidate.State == MobBehaviorState.Dead ||
                    entity.IsDestroyed || entity.OwnerUserId == selfEntity.OwnerUserId ||
                    IsCowardObjective(candidate, entity) != objective) continue;
                Fix64 distance = DistanceSquared3D(selfEntity.Position3D, entity.Position3D);
                if (distance > limit || distance > selectedDistance) continue;
                if (distance == selectedDistance && selected != null && candidate.EntityId > selected.EntityId) continue;
                selected = candidate;
                selectedDistance = distance;
            }
            return selected;
        }

        private static void UpdateThunderBirdFloat(MobAgentState agent, SimulationEntity entity)
        {
            const int hoverHeight = 3;
            Fix64 nextHeight = entity.Position3D.Z + SimulationWorld.FixedDeltaTime;
            if (nextHeight >= (Fix64)hoverHeight)
            {
                nextHeight = (Fix64)hoverHeight;
                agent.State = MobBehaviorState.Idle;
                agent.NextDecisionFrame = 0;
            }
            entity.SetPosition3D(new SimVector3(entity.Position.X, entity.Position.Y, nextHeight));
        }

        private static void UpdateAerialHover(SimulationEntity entity)
        {
            entity.SetVerticalVelocity(Fix64.Zero);
            Fix64 nextHeight = entity.Position3D.Z + SimulationWorld.FixedDeltaTime;
            if (nextHeight > (Fix64)3) nextHeight = (Fix64)3;
            entity.SetPosition3D(new SimVector3(entity.Position.X, entity.Position.Y, nextHeight));
        }

        private void ResolveThunderBirdDive(MobAgentState agent)
        {
            const int attackableHeight = 3;
            Fix64 threshold = (Fix64)0.5m;
            SimulationEntity entity = world.Entities[agent.EntityId];
            if (!TryGetLivingTarget(agent.TargetEntityId, out MobAgentState target))
            {
                agent.State = MobBehaviorState.Floating;
                agent.VerticalVelocity = Fix64.Zero;
                return;
            }
            agent.VerticalVelocity -= (Fix64)4 * SimulationWorld.FixedDeltaTime;
            Fix64 height = entity.Position3D.Z + agent.VerticalVelocity * SimulationWorld.FixedDeltaTime;
            if (height < Fix64.Zero) height = Fix64.Zero;
            Fix64 denominator = agent.DiveStartHeight;
            Fix64 progress = denominator == Fix64.Zero ? Fix64.One :
                (agent.DiveStartHeight - height) / denominator;
            if (progress < Fix64.Zero) progress = Fix64.Zero;
            if (progress > Fix64.One) progress = Fix64.One;
            SimVector2 targetPosition = world.Entities[target.EntityId].Position;
            Fix64 x = agent.DiveStartPosition.X + (targetPosition.X - agent.DiveStartPosition.X) * progress;
            Fix64 y = agent.DiveStartPosition.Y + (targetPosition.Y - agent.DiveStartPosition.Y) * progress;
            entity.SetPosition3D(new SimVector3(x, y, height));

            Fix64 contactRange = agent.Definition.ColliderRadius + target.Definition.ColliderRadius + threshold;
            if (DistanceSquared3D(entity.Position3D, world.Entities[target.EntityId].Position3D) >
                contactRange * contactRange) return;
            Fix64 scale = (agent.DiveStartHeight - height) / ((Fix64)attackableHeight - threshold);
            int damage = (int)(agent.Definition.Damage * scale);
            if (damage > 0) ApplyAttackDamage(agent.EntityId, target.EntityId, damage);
            agent.State = MobBehaviorState.Floating;
            agent.VerticalVelocity = Fix64.Zero;
            agent.TargetEntityId = -1;
            agent.SetPath(Array.Empty<GridPosition>());
        }

        private static bool IsCowardObjective(MobAgentState candidate, SimulationEntity entity) =>
            entity.PrefabId == Objects.SimulationPrefabRegistry.PlayerPrefabId ||
            candidate.Definition.Archetype == MobArchetype.Dummy ||
            candidate.Definition.Archetype == MobArchetype.Totem ||
            IsStationaryAttacker(candidate.Definition.Archetype);

        private SimVector2 ChaseVelocity(MobAgentState agent, SimulationEntity entity)
        {
            while (agent.PathIndex < agent.Path.Count && grid.ToGrid(entity.Position).Equals(agent.Path[agent.PathIndex])) agent.PathIndex++;
            if (agent.PathIndex >= agent.Path.Count) return SimVector2.Zero;
            SimVector2 destination = grid.CellCenter(agent.Path[agent.PathIndex]);
            Fix64 x = destination.X - entity.Position.X; Fix64 y = destination.Y - entity.Position.Y;
            Fix64 length = Fix64.Sqrt(x * x + y * y);
            Fix64 speed = agent.SnaredFrames > 0 || agent.SlowFrames > 0
                ? agent.Definition.Speed / (Fix64)2 : agent.Definition.Speed;
            return length == Fix64.Zero ? SimVector2.Zero : new SimVector2(x / length * speed, y / length * speed);
        }

        private static int StatusFrames(IReadOnlyDictionary<string, int> statuses, string id) =>
            statuses != null && statuses.TryGetValue(id, out int frames) ? frames : 0;

        private static int CeilingToInt(Fix64 value)
        {
            int truncated = (int)value;
            return value > (Fix64)truncated ? checked(truncated + 1) : truncated;
        }

        private bool IsFrenzyControllable(MobAgentState agent)
        {
            switch (world.Entities[agent.EntityId].PrefabId)
            {
                case Objects.SimulationPrefabRegistry.PlayerPrefabId:
                case "GroundCannon":
                case "ManaWell":
                case "PveNatureSlimeNest":
                case "PveWaterSlimeNest":
                case "PveVineColony":
                case "PveVineWitch":
                case "HealingTotem":
                case "LifeTree":
                case "RockTurret":
                    return false;
                default:
                    return true;
            }
        }

        private int EffectiveAttackCooldown(MobAgentState agent) =>
            EffectiveAttackCooldown(agent, agent.Definition.AttackCooldownFrames);

        private int EffectiveAttackCooldown(MobAgentState agent, int baseFrames)
        {
            Fix64 multiplier = Fix64.One;
            int speedBuffRevision = CanModifyAttackInterval(agent)
                ? Math.Max(agent.FrenzyFrames > 0 ? agent.FrenzyRevision : 0,
                    agent.InspiredFrames > 0 ? agent.InspiredRevision : 0)
                : 0;
            bool behaviorMob = world.Entities[agent.EntityId]
                .HasComponent(SimulationPrefabRegistry.BehaviorMobComponentId);
            if (behaviorMob && agent.SnaredFrames > 0 &&
                agent.SnaredRevision >= speedBuffRevision)
                multiplier += (Fix64)0.5m;
            else if (speedBuffRevision > 0)
                multiplier -= (Fix64)(1m / 3m);
            int value = checked((int)Fix64.Ceiling(baseFrames * multiplier));
            return Math.Max(1, value);
        }

        private bool CanModifyAttackInterval(MobAgentState agent)
        {
            if (world.Entities[agent.EntityId]
                .HasComponent(SimulationPrefabRegistry.BehaviorMobComponentId)) return true;
            switch (agent.Definition.Archetype)
            {
                case MobArchetype.Turret:
                case MobArchetype.LightningTower:
                case MobArchetype.BubbleGenerator:
                case MobArchetype.StationarySummoner:
                case MobArchetype.Tower:
                    return true;
                default:
                    return false;
            }
        }

        private static bool CanReceiveEffects(SimulationEntity entity) =>
            entity.HasComponent(SimulationPrefabRegistry.CommonEffectReceiverComponentId) ||
            entity.HasComponent(SimulationPrefabRegistry.BuildingEffectReceiverComponentId);

        private static bool IsAirborne(SimulationEntity entity) =>
            entity.Position3D.Z >= (Fix64)2;

        private bool InRange(int leftId, int rightId, Fix64 range)
        {
            Fix64 targetRadius = agents.TryGetValue(rightId, out MobAgentState target)
                ? target.Definition.ColliderRadius : world.Entities[rightId].Radius;
            Fix64 effectiveRange = range + targetRadius;
            return DistanceSquared3D(world.Entities[leftId].Position3D,
                world.Entities[rightId].Position3D) < effectiveRange * effectiveRange;
        }
        private bool InEntryRange(int leftId, int rightId, Fix64 range)
        {
            Fix64 entryRange = range > (Fix64)0.1m ? range - (Fix64)0.1m : Fix64.Zero;
            return InRange(leftId, rightId, entryRange);
        }
        private bool InCenterRange(int leftId, int rightId, Fix64 range) =>
            DistanceSquared3D(world.Entities[leftId].Position3D,
                world.Entities[rightId].Position3D) < range * range;
        private static bool IsStationaryAttacker(MobArchetype archetype) =>
            archetype == MobArchetype.Turret || archetype == MobArchetype.Tower ||
            archetype == MobArchetype.LightningTower || archetype == MobArchetype.StationarySummoner;
        private static bool IsCoward(MobArchetype archetype) =>
            archetype == MobArchetype.Coward || archetype == MobArchetype.NonAttackingCoward;
        private static Fix64 DistanceSquared(SimVector2 left, SimVector2 right)
        { Fix64 x = left.X - right.X; Fix64 y = left.Y - right.Y; return x * x + y * y; }
        private static Fix64 DistanceSquared3D(SimVector3 left, SimVector3 right)
        {
            Fix64 x = left.X - right.X; Fix64 y = left.Y - right.Y; Fix64 z = left.Z - right.Z;
            return x * x + y * y + z * z;
        }
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

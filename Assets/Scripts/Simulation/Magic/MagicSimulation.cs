using System;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Physics;

namespace GameScene.Simulation.Magic
{
    public readonly struct MagicSpawnEvent
    {
        public int EntityId { get; }
        public long OwnerUserId { get; }
        public MagicDefinition Definition { get; }

        public MagicSpawnEvent(int entityId, long ownerUserId, MagicDefinition definition)
        {
            EntityId = entityId;
            OwnerUserId = ownerUserId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }
    }

    public readonly struct MagicCastCommand
    {
        public long UserId { get; }
        public int RequestId { get; }
        public string MagicName { get; }
        public SimVector2 Target { get; }
        public int CasterEntityId { get; }

        public MagicCastCommand(long userId, int requestId, string magicName, SimVector2 target,
            int casterEntityId = -1)
        {
            if (string.IsNullOrWhiteSpace(magicName)) throw new ArgumentException("Server magic name is required", nameof(magicName));
            UserId = userId; RequestId = requestId; MagicName = magicName; Target = target;
            CasterEntityId = casterEntityId;
        }
    }

    public sealed class MagicCombatState
    {
        private readonly SortedDictionary<string, int> statuses =
            new SortedDictionary<string, int>(StringComparer.Ordinal);
        private readonly SortedDictionary<string, int> statusRevisions =
            new SortedDictionary<string, int>(StringComparer.Ordinal);
        private int nextStatusRevision;
        public int EntityId { get; }
        public int MaxHealth { get; }
        public int Health { get; private set; } = 100;
        public IReadOnlyDictionary<string, int> Statuses => statuses;
        public IReadOnlyDictionary<string, int> StatusRevisions => statusRevisions;
        public int BubbleShieldFrames => statuses.TryGetValue("bubble", out int frames) ? frames : 0;
        private int burnDotElapsedFrames;
        private int sandstormElapsedFrames;
        private int leafHealElapsedFrames;
        private int natureWetHealElapsedFrames;
        private int buildingSnareElapsedFrames;
        internal SimVector2 KnockbackDirection { get; private set; }
        internal Fix64 KnockbackProximity { get; private set; }
        internal int KnockbackFrames => statuses.TryGetValue("knockback", out int frames) ? frames : 0;
        internal int RallySourceEntityId { get; private set; } = -1;
        internal bool LifetimeRecovered { get; private set; }
        internal MagicCombatState(int entityId, int health)
        {
            if (health < 1) throw new ArgumentOutOfRangeException(nameof(health));
            EntityId = entityId; MaxHealth = health; Health = health;
        }
        internal void Damage(int amount)
        {
            if (amount > 0 && RemoveStatus("bubble")) return;
            Health = Math.Max(0, Health - amount);
        }
        internal void ResolveHealth(int value) { Health = Math.Max(0, value); }
        internal void Heal(int amount) { Health = Math.Min(MaxHealth, Health + Math.Max(0, amount)); }
        internal void ApplyStatus(string id, int frames)
        {
            if (string.IsNullOrEmpty(id) || frames <= 0) return;
            if (id == "shock" && statuses.ContainsKey("shock")) frames = 60;
            if (id == "burn")
            {
                RemoveStatus("snared");
                bool cancelled = RemoveStatus("wet");
                if (!cancelled) Refresh("burn", frames);
                Refresh("burn-dot", 60);
                return;
            }
            if (id == "wet")
            {
                bool cancelled = RemoveStatus("burn");
                if (!cancelled) Refresh("wet", frames);
                return;
            }
            Refresh(id, frames);
        }
        private void Refresh(string id, int frames)
        {
            if (!statuses.TryGetValue(id, out int current) || current < frames) statuses[id] = frames;
            statusRevisions[id] = ++nextStatusRevision;
        }
        internal void ResolveBubbleShield(int frames)
        {
            if (frames <= 0) RemoveStatus("bubble");
            else Refresh("bubble", frames);
        }
        internal void ApplyKnockback(SimVector2 direction, Fix64 proximity)
        {
            if (statuses.ContainsKey("knockback")) return;
            KnockbackDirection = direction;
            KnockbackProximity = proximity < (Fix64)0.2m ? (Fix64)0.2m : proximity;
            Refresh("knockback", 10);
        }
        internal void ApplyBuildingStatus(string id, int frames)
        {
            if (string.IsNullOrEmpty(id) || frames <= 0) return;
            if (id == "burn")
            {
                bool cancelled = RemoveStatus("wet");
                if (!cancelled) Refresh("burn", frames);
                return;
            }
            if (id == "wet")
            {
                bool cancelled = RemoveStatus("burn");
                if (!cancelled) Refresh("wet", frames);
                return;
            }
            Refresh(id, frames);
        }
        internal void RallyTo(int sourceEntityId) { RallySourceEntityId = sourceEntityId; }
        internal void RecoverLifetime() { LifetimeRecovered = true; }
        internal void ClearFrameSignals() { RallySourceEntityId = -1; LifetimeRecovered = false; }
        internal void Step(int burnDotDamage)
        {
            if (statuses.ContainsKey("burn-dot"))
            {
                burnDotElapsedFrames++;
                if (burnDotElapsedFrames % 20 == 0) Damage(burnDotDamage);
            }
            else burnDotElapsedFrames = 0;
            if (statuses.ContainsKey("sandstorm"))
            {
                sandstormElapsedFrames++;
                if (sandstormElapsedFrames == 10) Damage(1);
            }
            else sandstormElapsedFrames = 0;
            if (statuses.ContainsKey("leaf-heal"))
            {
                leafHealElapsedFrames++;
                if (leafHealElapsedFrames == 60) Heal(1);
            }
            else leafHealElapsedFrames = 0;
            if (statuses.ContainsKey("nature-wet-heal"))
            {
                natureWetHealElapsedFrames++;
                if (natureWetHealElapsedFrames % 20 == 0) Heal(1);
            }
            else natureWetHealElapsedFrames = 0;
            if (statuses.ContainsKey("building-snare-dot"))
            {
                buildingSnareElapsedFrames++;
                if (buildingSnareElapsedFrames == 60) Heal(1);
            }
            else buildingSnareElapsedFrames = 0;
            List<string> keys = new List<string>(statuses.Keys);
            for (int index = 0; index < keys.Count; index++)
            {
                if (keys[index] == "bubble") continue;
                int remaining = statuses[keys[index]] - 1;
                if (remaining <= 0) RemoveStatus(keys[index]); else statuses[keys[index]] = remaining;
            }
        }
        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteInt32(EntityId); writer.WriteInt32(MaxHealth); writer.WriteInt32(Health);
            writer.WriteInt32(burnDotElapsedFrames); writer.WriteInt32(sandstormElapsedFrames);
            writer.WriteInt32(leafHealElapsedFrames);
            writer.WriteInt32(natureWetHealElapsedFrames); writer.WriteInt32(buildingSnareElapsedFrames);
            writer.WriteFixed64(KnockbackDirection.X); writer.WriteFixed64(KnockbackDirection.Y);
            writer.WriteFixed64(KnockbackProximity);
            writer.WriteInt32(RallySourceEntityId);
            writer.WriteBoolean(LifetimeRecovered);
            writer.WriteInt32(nextStatusRevision);
            writer.WriteInt32(statuses.Count);
            foreach (KeyValuePair<string, int> pair in statuses)
            {
                writer.WriteString(pair.Key); writer.WriteInt32(pair.Value);
                writer.WriteInt32(statusRevisions[pair.Key]);
            }
        }

        private bool RemoveStatus(string id)
        {
            statusRevisions.Remove(id);
            return statuses.Remove(id);
        }
    }

    public sealed class MagicSimulation
    {
        private sealed class Instance
        {
            public int EntityId;
            public long OwnerUserId;
            public MagicDefinition Definition;
            public int RemainingFrames;
            public int ActivationRemainingFrames;
            public int EffectElapsedFrames;
            public int CurrentDamage;
            public int CooldownFrames;
            public int PendingTargetId = -1;
            public int HitCount;
            public int BounceCount;
            public SimVector2 Direction;
            public SimVector2 PreviousPosition;
            public SimVector2 LastPathSpawnPosition;
            public readonly SortedSet<int> HitEntityIds = new SortedSet<int>();
            public readonly SortedDictionary<int, SimVector2> OrbitOffsets =
                new SortedDictionary<int, SimVector2>();
        }

        private sealed class VineSequence
        {
            public long OwnerUserId;
            public MagicDefinition Definition;
            public SimVector2 FirstPosition;
            public SimVector2 Direction;
            public int RemainingSpawnCount;
            public int NextSpawnIndex;
            public Fix64 Elapsed;
            public readonly SortedSet<int> HitEntityIds = new SortedSet<int>();
        }

        private sealed class FieldInstance
        {
            public int EntityId;
            public FieldDefinition Definition;
            public int RemainingFrames;
        }

        private sealed class AuxiliaryInstance
        {
            public int EntityId;
            public AuxiliaryPrefabDefinition Definition;
            public int RemainingFrames;
            public int ElapsedFrames;
            public SimVector2 Direction;
            public SimVector2 StartPosition;
            public Fix64 TravelDistance;
        }

        private readonly long sessionSeed;
        private readonly SimulationWorld world;
        private readonly ProductionMagicCatalog catalog;
        private readonly List<Instance> instances = new List<Instance>();
        private readonly List<VineSequence> vineSequences = new List<VineSequence>();
        private readonly List<FieldInstance> fieldInstances = new List<FieldInstance>();
        private readonly HashSet<int> registeredFieldEntityIds = new HashSet<int>();
        private readonly List<AuxiliaryInstance> auxiliaryInstances = new List<AuxiliaryInstance>();
        private readonly HashSet<int> registeredAuxiliaryEntityIds = new HashSet<int>();
        private readonly List<MagicSpawnEvent> spawnedThisFrame = new List<MagicSpawnEvent>();
        private readonly SortedDictionary<int, MagicCombatState> combatants =
            new SortedDictionary<int, MagicCombatState>();

        public SimulationWorld World => world;
        public IReadOnlyDictionary<int, MagicCombatState> Combatants => combatants;
        public IReadOnlyList<MagicSpawnEvent> SpawnedThisFrame => spawnedThisFrame;

        public MagicSimulation(long sessionSeed, SimulationWorld world, ProductionMagicCatalog catalog)
        {
            this.sessionSeed = sessionSeed;
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            RegisterCombatants();
        }

        public void RegisterCombatant(int entityId, int health = 100)
        {
            if (entityId < 0 || entityId >= world.Entities.Count || world.Entities[entityId].IsDestroyed)
                throw new InvalidOperationException("Cannot register missing combatant: " + entityId);
            if (!combatants.ContainsKey(entityId)) combatants.Add(entityId, new MagicCombatState(entityId, health));
        }

        public void ResolveBubbleShield(int entityId, int frames)
        {
            if (!combatants.TryGetValue(entityId, out MagicCombatState target)) return;
            target.ResolveBubbleShield(frames);
        }

        public void ApplyExternalStatus(int entityId, string statusId, int frames)
        {
            if (combatants.TryGetValue(entityId, out MagicCombatState target))
                ApplyStatusToTarget(entityId, target, statusId, frames);
        }

        public void Step(IReadOnlyList<MagicCastCommand> casts)
        {
            Step(casts, Array.Empty<SimulationInput>());
        }

        public void Step(IReadOnlyList<MagicCastCommand> casts, IReadOnlyList<SimulationInput> physicsInputs)
        {
            if (casts == null) throw new ArgumentNullException(nameof(casts));
            if (physicsInputs == null) throw new ArgumentNullException(nameof(physicsInputs));
            spawnedThisFrame.Clear();
            foreach (MagicCombatState state in combatants.Values) state.ClearFrameSignals();
            RegisterCombatants();
            RegisterFields();
            RegisterAuxiliaryPrefabs();
            List<MagicCastCommand> ordered = new List<MagicCastCommand>(casts);
            ordered.Sort((left, right) =>
            {
                int user = left.UserId.CompareTo(right.UserId);
                return user != 0 ? user : left.RequestId.CompareTo(right.RequestId);
            });
            for (int index = 1; index < ordered.Count; index++)
                if (ordered[index - 1].UserId == ordered[index].UserId &&
                    ordered[index - 1].RequestId == ordered[index].RequestId)
                    throw new InvalidOperationException("Duplicate magic cast request: " + ordered[index].RequestId);
            for (int index = 0; index < ordered.Count; index++) Cast(ordered[index]);

            CaptureInstancePositions();
            world.Step(physicsInputs);
            ApplyFieldCollisions();
            ApplyProjectileHits();
            ApplyDropCollisions();
            ApplyAuxiliaryCollisions();
            MovePushedTargets();
            TickInstances();
            ApplyKnockbackMovement();
            TickVineSequences();
            TickFields();
            TickAuxiliaryPrefabs();
            foreach (MagicCombatState state in combatants.Values)
                state.Step(ScaledDamage(state.EntityId, state, 1, SimulationElement.Fire));
        }

        public ulong CalculateStateHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter();
            writer.WriteUInt64(world.CalculateStateHash());
            writer.WriteInt64(sessionSeed);
            writer.WriteInt32(combatants.Count);
            foreach (MagicCombatState state in combatants.Values) state.WriteState(writer);
            instances.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            writer.WriteInt32(instances.Count);
            for (int index = 0; index < instances.Count; index++)
            {
                Instance instance = instances[index];
                writer.WriteInt32(instance.EntityId); writer.WriteInt64(instance.OwnerUserId);
                writer.WriteInt32(instance.Definition.Id); writer.WriteInt32(instance.RemainingFrames);
                writer.WriteInt32(instance.ActivationRemainingFrames);
                writer.WriteInt32(instance.EffectElapsedFrames);
                writer.WriteInt32(instance.CurrentDamage); writer.WriteInt32(instance.CooldownFrames);
                writer.WriteInt32(instance.PendingTargetId);
                writer.WriteInt32(instance.HitCount); writer.WriteInt32(instance.BounceCount);
                writer.WriteFixed64(instance.Direction.X);
                writer.WriteFixed64(instance.Direction.Y); writer.WriteFixed64(instance.PreviousPosition.X);
                writer.WriteFixed64(instance.PreviousPosition.Y); writer.WriteFixed64(instance.LastPathSpawnPosition.X);
                writer.WriteFixed64(instance.LastPathSpawnPosition.Y); writer.WriteInt32(instance.HitEntityIds.Count);
                foreach (int entityId in instance.HitEntityIds) writer.WriteInt32(entityId);
                writer.WriteInt32(instance.OrbitOffsets.Count);
                foreach (KeyValuePair<int, SimVector2> orbit in instance.OrbitOffsets)
                {
                    writer.WriteInt32(orbit.Key); writer.WriteFixed64(orbit.Value.X);
                    writer.WriteFixed64(orbit.Value.Y);
                }
            }
            writer.WriteInt32(vineSequences.Count);
            for (int index = 0; index < vineSequences.Count; index++)
            {
                VineSequence sequence = vineSequences[index];
                writer.WriteInt64(sequence.OwnerUserId); writer.WriteInt32(sequence.Definition.Id);
                writer.WriteFixed64(sequence.FirstPosition.X); writer.WriteFixed64(sequence.FirstPosition.Y);
                writer.WriteFixed64(sequence.Direction.X); writer.WriteFixed64(sequence.Direction.Y);
                writer.WriteInt32(sequence.RemainingSpawnCount); writer.WriteInt32(sequence.NextSpawnIndex);
                writer.WriteFixed64(sequence.Elapsed); writer.WriteInt32(sequence.HitEntityIds.Count);
                foreach (int entityId in sequence.HitEntityIds) writer.WriteInt32(entityId);
            }
            fieldInstances.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            writer.WriteInt32(fieldInstances.Count);
            for (int index = 0; index < fieldInstances.Count; index++)
            {
                writer.WriteInt32(fieldInstances[index].EntityId);
                writer.WriteString(fieldInstances[index].Definition.PrefabId);
                writer.WriteInt32(fieldInstances[index].RemainingFrames);
            }
            auxiliaryInstances.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            writer.WriteInt32(auxiliaryInstances.Count);
            for (int index = 0; index < auxiliaryInstances.Count; index++)
            {
                AuxiliaryInstance value = auxiliaryInstances[index];
                writer.WriteInt32(value.EntityId); writer.WriteInt32((int)value.Definition.Kind);
                writer.WriteInt32(value.RemainingFrames); writer.WriteInt32(value.ElapsedFrames);
                writer.WriteFixed64(value.Direction.X); writer.WriteFixed64(value.Direction.Y);
                writer.WriteFixed64(value.StartPosition.X); writer.WriteFixed64(value.StartPosition.Y);
                writer.WriteFixed64(value.TravelDistance);
            }
            return writer.Hash;
        }

        private void RegisterAuxiliaryPrefabs()
        {
            for (int index = 0; index < world.Entities.Count; index++)
            {
                SimulationEntity entity = world.Entities[index];
                if (entity.IsDestroyed || registeredAuxiliaryEntityIds.Contains(entity.Id) ||
                    !catalog.TryGetAuxiliaryPrefab(entity.PrefabId, out AuxiliaryPrefabDefinition definition)) continue;
                if (FindInstance(entity.Id) != null) continue;
                registeredAuxiliaryEntityIds.Add(entity.Id);
                AuxiliaryInstance value = new AuxiliaryInstance { EntityId = entity.Id, Definition = definition };
                if (definition.Kind == AuxiliaryPrefabKind.CraterEmber)
                {
                    Fix64 x = (Fix64)(world.DrawRandomUInt32() % 2001) - (Fix64)1000;
                    Fix64 y = (Fix64)(world.DrawRandomUInt32() % 2001) - (Fix64)1000;
                    Fix64 length = Fix64.Sqrt(x * x + y * y);
                    value.Direction = length == Fix64.Zero ? new SimVector2(Fix64.One, Fix64.Zero) :
                        new SimVector2(x / length, y / length);
                    Fix64 distance = Fix64.One + (Fix64)(world.DrawRandomUInt32() % 1001) /
                        (Fix64)1000 * (definition.EffectRadius - Fix64.One);
                    value.StartPosition = entity.Position;
                    value.TravelDistance = distance;
                    value.RemainingFrames = Math.Max(1, CeilingToInt(distance /
                        ((Fix64)4 * SimulationWorld.FixedDeltaTime)));
                    entity.SetVelocity(value.Direction * (Fix64)4);
                }
                else if (definition.Kind == AuxiliaryPrefabKind.OnStartArea)
                {
                    value.RemainingFrames = definition.LifetimeFrames;
                    ApplyAuxiliaryStartArea(entity, definition);
                }
                else if (definition.Kind == AuxiliaryPrefabKind.FallingDrop)
                {
                    value.RemainingFrames = definition.LifetimeFrames;
                    if (entity.Position3D.Z <= Fix64.Zero)
                        StartFalling(entity, definition.LifetimeFrames - 1);
                }
                auxiliaryInstances.Add(value);
            }
        }

        private void ApplyAuxiliaryStartArea(SimulationEntity source,
            AuxiliaryPrefabDefinition definition)
        {
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                SimulationEntity target = world.Entities[pair.Key];
                if (target.IsDestroyed) continue;
                Fix64 dx = target.Position.X - source.Position.X;
                Fix64 dy = target.Position.Y - source.Position.Y;
                Fix64 dz = target.Position3D.Z - source.Position3D.Z;
                Fix64 overlap = definition.EffectRadius + target.Radius;
                if (dx * dx + dy * dy + dz * dz >= overlap * overlap) continue;
                if (source.OwnerUserId == 0 || target.OwnerUserId != source.OwnerUserId)
                    Damage(pair.Key, pair.Value, definition.Damage, source.Elements);
                ApplyStatusToTarget(pair.Key, pair.Value, definition.StatusId, 60);
            }
        }

        private void ApplyAuxiliaryCollisions()
        {
            for (int eventIndex = 0; eventIndex < world.CollisionEvents.Count; eventIndex++)
            {
                SimulationCollisionEvent collision = world.CollisionEvents[eventIndex];
                if (collision.Type != SimulationCollisionEventType.Enter) continue;
                ResolveAuxiliaryCollision(collision.EntityA, collision.EntityB);
                ResolveAuxiliaryCollision(collision.EntityB, collision.EntityA);
            }
        }

        private void ResolveAuxiliaryCollision(int sourceId, int otherId)
        {
            AuxiliaryInstance source = FindAuxiliary(sourceId);
            if (source == null || world.Entities[sourceId].IsDestroyed) return;
            if (source.Definition.Kind == AuxiliaryPrefabKind.CraterEmber)
            {
                if (!combatants.TryGetValue(otherId, out MagicCombatState target) ||
                    world.Entities[otherId].OwnerUserId == world.Entities[sourceId].OwnerUserId) return;
                Damage(otherId, target, source.Definition.Damage,
                    world.Entities[sourceId].Elements);
            }
            else if (source.Definition.Kind == AuxiliaryPrefabKind.Rune) ExplodeRune(source);
            else if (source.Definition.Kind == AuxiliaryPrefabKind.OnStartArea)
            {
                if (combatants.TryGetValue(otherId, out MagicCombatState target))
                    ApplyStatusToTarget(otherId, target, source.Definition.StatusId, 60);
                return;
            }
            else if (source.Definition.Kind == AuxiliaryPrefabKind.FallingDrop)
            {
                if (!combatants.TryGetValue(otherId, out MagicCombatState target)) return;
                Damage(otherId, target, source.Definition.Damage,
                    world.Entities[sourceId].Elements);
            }
            else return;
            world.Destroy(sourceId); source.RemainingFrames = 0;
        }

        private void ExplodeRune(AuxiliaryInstance rune)
        {
            SimVector2 center = world.Entities[rune.EntityId].Position;
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                SimulationEntity target = world.Entities[pair.Key];
                if (target.IsDestroyed) continue;
                Fix64 dx = target.Position.X - center.X;
                Fix64 dy = target.Position.Y - center.Y;
                Fix64 dz = target.Position3D.Z - world.Entities[rune.EntityId].Position3D.Z;
                Fix64 overlap = rune.Definition.EffectRadius + target.Radius;
                if (dx * dx + dy * dy + dz * dz < overlap * overlap)
                    Damage(pair.Key, pair.Value, rune.Definition.Damage,
                        world.Entities[rune.EntityId].Elements);
            }
        }

        private void TickAuxiliaryPrefabs()
        {
            for (int index = auxiliaryInstances.Count - 1; index >= 0; index--)
            {
                AuxiliaryInstance value = auxiliaryInstances[index];
                if (world.Entities[value.EntityId].IsDestroyed)
                {
                    auxiliaryInstances.RemoveAt(index); continue;
                }
                if (value.Definition.Kind == AuxiliaryPrefabKind.Rune) continue;
                if (value.Definition.Kind == AuxiliaryPrefabKind.CraterEmber)
                {
                    value.ElapsedFrames++;
                    value.RemainingFrames--;
                    SimulationEntity ember = world.Entities[value.EntityId];
                    if (value.RemainingFrames <= 0)
                    {
                        SimVector2 landing = value.StartPosition + value.Direction * value.TravelDistance;
                        ember.SetPosition3D(new SimVector3(landing.X, landing.Y, Fix64.Zero));
                        ember.SetVelocity(SimVector2.Zero);
                        world.Spawn("FireField", 0, landing);
                        world.Destroy(value.EntityId);
                        auxiliaryInstances.RemoveAt(index);
                        continue;
                    }
                    Fix64 horizontalDistance = (Fix64)4 * SimulationWorld.FixedDeltaTime * value.ElapsedFrames;
                    Fix64 progress = horizontalDistance / value.TravelDistance;
                    Fix64 height = (Fix64)12 * progress * (Fix64.One - progress);
                    SimVector2 position = value.StartPosition + value.Direction * horizontalDistance;
                    ember.SetPosition3D(new SimVector3(position.X, position.Y,
                        height > Fix64.Zero ? height : Fix64.Zero));
                    continue;
                }
                if (--value.RemainingFrames > 0) continue;
                SimulationEntity entity = world.Entities[value.EntityId];
                world.Destroy(value.EntityId);
                auxiliaryInstances.RemoveAt(index);
            }
        }

        private AuxiliaryInstance FindAuxiliary(int entityId)
        {
            for (int index = 0; index < auxiliaryInstances.Count; index++)
                if (auxiliaryInstances[index].EntityId == entityId) return auxiliaryInstances[index];
            return null;
        }

        private void Cast(MagicCastCommand command)
        {
            MagicDefinition definition = catalog.GetRequired(command.MagicName);
            SimulationEntity caster = command.CasterEntityId >= 0
                ? FindCaster(command.CasterEntityId, command.UserId) : FindCaster(command.UserId);
            SimVector2 target = ClampTarget(caster.Position, command.Target, definition.Range);
            if (definition.ExecutionKind == MagicExecutionKind.VineLine ||
                definition.ExecutionKind == MagicExecutionKind.VineFan)
            {
                CastVines(command.UserId, caster.Position, target, definition);
                return;
            }
            DeterministicRandom random = new DeterministicRandom(DeriveSeed(command, definition.Name));
            for (int index = 0; index < definition.SpawnCount; index++)
            {
                SimVector2 position = IsMovingShot(definition.ExecutionKind)
                    ? caster.Position : AddStableOffset(target, random, definition.SpawnCount > 1);
                SimulationEntity entity = world.Spawn(definition.PrefabId, command.UserId, position);
                spawnedThisFrame.Add(new MagicSpawnEvent(entity.Id, command.UserId, definition));
                SimVector2 shotDirection = Direction(caster.Position, target);
                if (IsMovingShot(definition.ExecutionKind))
                    entity.SetVelocity(shotDirection * definition.Speed);
                else if (definition.ExecutionKind == MagicExecutionKind.MovingTornado)
                    entity.SetVelocity(Direction(position, new SimVector2((Fix64)9, (Fix64)5)) * definition.Speed);
                if (IsFallingMagic(definition.ExecutionKind))
                    StartFalling(entity, definition.ActivationFrames -
                        (definition.ExecutionKind == MagicExecutionKind.RallyingTorch ? 0 : 1));
                if (definition.LifetimeFrames > 0 || IsMovingShot(definition.ExecutionKind))
                    instances.Add(new Instance
                    {
                        EntityId = entity.Id, OwnerUserId = command.UserId,
                        Definition = definition,
                        RemainingFrames = definition.LifetimeFrames > 0 ? definition.LifetimeFrames : -1,
                        ActivationRemainingFrames = definition.ActivationFrames,
                        CurrentDamage = definition.Damage,
                        Direction = shotDirection,
                        PreviousPosition = entity.Position,
                        LastPathSpawnPosition = entity.Position
                    });
            }
            if (definition.ExecutionKind == MagicExecutionKind.LightningColumn)
                ApplyLightningColumn(command.UserId, target, definition);
        }

        private void ApplyLightningColumn(long ownerUserId, SimVector2 center,
            MagicDefinition definition)
        {
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                SimulationEntity target = world.Entities[pair.Key];
                if (target.IsDestroyed || target.OwnerUserId == ownerUserId) continue;
                Fix64 x = target.Position.X;
                Fix64 y = target.Position.Y;
                Fix64 height = target.Position3D.Z;
                if (x < center.X - definition.Radius || x > center.X + definition.Radius ||
                    y < center.Y - definition.Radius || y > center.Y + definition.Radius ||
                    height < Fix64.Zero || height > (Fix64)10) continue;
                Damage(pair.Key, pair.Value, definition.Damage,
                    SimulationPrefabRegistry.ElementsFor(definition.PrefabId));
            }
        }

        private void CastVines(long ownerUserId, SimVector2 origin, SimVector2 target,
            MagicDefinition definition)
        {
            SimVector2 center = Direction(origin, target);
            if (center.Equals(SimVector2.Zero)) return;
            if (definition.ExecutionKind == MagicExecutionKind.VineLine)
            {
                StartVineSequence(ownerUserId, origin, center, definition);
                return;
            }
            StartVineSequence(ownerUserId, origin,
                Rotate(center, (Fix64)0.8660254038m, (Fix64)(-0.5m)), definition);
            StartVineSequence(ownerUserId, origin, center, definition);
            StartVineSequence(ownerUserId, origin, Rotate(center, (Fix64)0.8660254038m, (Fix64)0.5m), definition);
        }

        private void StartVineSequence(long ownerUserId, SimVector2 origin, SimVector2 direction,
            MagicDefinition definition)
        {
            VineSequence sequence = new VineSequence
            {
                OwnerUserId = ownerUserId,
                Definition = definition,
                Direction = direction,
                FirstPosition = origin + direction,
                RemainingSpawnCount = 5,
                NextSpawnIndex = 1
            };
            SpawnVine(sequence, sequence.FirstPosition);
            vineSequences.Add(sequence);
        }

        private void SpawnVine(VineSequence sequence, SimVector2 position)
        {
            SimulationEntity entity = world.Spawn(sequence.Definition.PrefabId,
                sequence.OwnerUserId, position);
            spawnedThisFrame.Add(new MagicSpawnEvent(entity.Id, sequence.OwnerUserId, sequence.Definition));
            if (sequence.Definition.LifetimeFrames > 0)
                instances.Add(new Instance
                {
                    EntityId = entity.Id, OwnerUserId = sequence.OwnerUserId,
                    Definition = sequence.Definition,
                    RemainingFrames = sequence.Definition.LifetimeFrames,
                    ActivationRemainingFrames = sequence.Definition.ActivationFrames
                });
            ApplyArea(sequence.OwnerUserId, position, sequence.Definition, sequence.HitEntityIds);
        }

        private void TickVineSequences()
        {
            for (int index = vineSequences.Count - 1; index >= 0; index--)
            {
                VineSequence sequence = vineSequences[index];
                sequence.Elapsed += SimulationWorld.FixedDeltaTime;
                while (sequence.Elapsed >= (Fix64)0.12m && sequence.RemainingSpawnCount > 0)
                {
                    sequence.Elapsed -= (Fix64)0.12m;
                    SpawnVine(sequence, sequence.FirstPosition +
                        sequence.Direction * sequence.NextSpawnIndex);
                    sequence.NextSpawnIndex++;
                    sequence.RemainingSpawnCount--;
                }
                if (sequence.RemainingSpawnCount == 0) vineSequences.RemoveAt(index);
            }
        }

        private void ApplyArea(long ownerUserId, SimVector2 target, MagicDefinition definition)
        {
            ApplyArea(ownerUserId, target, definition, null);
        }

        private void ApplyArea(long ownerUserId, SimVector2 target, MagicDefinition definition,
            ISet<int> hitEntityIds)
        {
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                SimulationEntity entity = world.Entities[pair.Key];
                if (entity.IsDestroyed || entity.OwnerUserId == ownerUserId && !AffectsAllies(definition)) continue;
                if (hitEntityIds != null && hitEntityIds.Contains(pair.Key)) continue;
                SimVector2 delta = new SimVector2(entity.Position.X - target.X, entity.Position.Y - target.Y);
                Fix64 overlap = definition.Radius + entity.Radius;
                Fix64 dz = entity.Position3D.Z;
                if (delta.X * delta.X + delta.Y * delta.Y + dz * dz >= overlap * overlap) continue;
                Damage(pair.Key, pair.Value, definition.Damage,
                    SimulationPrefabRegistry.ElementsFor(definition.PrefabId));
                if (definition.VerticalImpulse > Fix64.Zero &&
                    entity.HasComponent(SimulationPrefabRegistry.ZPhysicsComponentId))
                    entity.AddVerticalImpulse(definition.VerticalImpulse);
                if (definition.ExecutionKind != MagicExecutionKind.Explosion && definition.StatusId == "knockback")
                {
                    SimVector2 direction = Direction(target, entity.Position);
                    Fix64 distance = Fix64.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
                    ApplyKnockbackToTarget(pair.Key, pair.Value, direction,
                        definition.Radius <= Fix64.Zero ? Fix64.One : distance / definition.Radius);
                }
                else if (definition.ExecutionKind != MagicExecutionKind.Explosion)
                    ApplyStatusToTarget(pair.Key, pair.Value, definition.StatusId, definition.StatusFrames);
                hitEntityIds?.Add(pair.Key);
            }
            ApplyEffectToFields(definition.StatusId, target, definition.Radius);
        }

        private void RegisterFields()
        {
            for (int index = 0; index < world.Entities.Count; index++)
            {
                SimulationEntity entity = world.Entities[index];
                if (entity.IsDestroyed || registeredFieldEntityIds.Contains(entity.Id) ||
                    !catalog.TryGetField(entity.PrefabId, out FieldDefinition definition)) continue;
                registeredFieldEntityIds.Add(entity.Id);
                fieldInstances.Add(new FieldInstance
                {
                    EntityId = entity.Id, Definition = definition,
                    RemainingFrames = definition.LifetimeFrames
                });
            }
        }

        private void ApplyFieldCollisions()
        {
            for (int index = 0; index < world.CollisionEvents.Count; index++)
            {
                SimulationCollisionEvent collision = world.CollisionEvents[index];
                if (collision.Type != SimulationCollisionEventType.Enter) continue;
                ApplyFieldCollision(collision.EntityA, collision.EntityB);
                ApplyFieldCollision(collision.EntityB, collision.EntityA);
            }
        }

        private void ApplyFieldCollision(int fieldEntityId, int otherEntityId)
        {
            FieldInstance field = FindField(fieldEntityId);
            if (field == null || world.Entities[fieldEntityId].IsDestroyed) return;
            if (combatants.TryGetValue(otherEntityId, out MagicCombatState target))
            {
                ApplyStatusToTarget(otherEntityId, target, field.Definition.StatusId,
                    StatusDuration(field.Definition.StatusId));
                if (field.Definition.PrefabId == "LeafField")
                    ApplyStatusToTarget(otherEntityId, target, "leaf-heal", 60);
            }
            Instance magic = FindInstance(otherEntityId);
            if (magic != null) TransformField(field, magic.Definition.StatusId);
        }

        private void ApplyEffectToFields(string statusId, SimVector2 center, Fix64 radius)
        {
            if (string.IsNullOrEmpty(statusId)) return;
            for (int index = 0; index < fieldInstances.Count; index++)
            {
                FieldInstance field = fieldInstances[index];
                SimulationEntity entity = world.Entities[field.EntityId];
                if (entity.IsDestroyed) continue;
                Fix64 dx = entity.Position.X - center.X;
                Fix64 dy = entity.Position.Y - center.Y;
                Fix64 dz = entity.Position3D.Z;
                Fix64 overlap = radius + entity.Radius;
                if (dx * dx + dy * dy + dz * dz < overlap * overlap)
                    TransformField(field, statusId);
            }
        }

        private void TransformField(FieldInstance field, string statusId)
        {
            string replacement = field.Definition.PrefabId == "LeafField" && statusId == "burn"
                ? "FireField" : field.Definition.PrefabId == "WaterField" && statusId == "shock"
                    ? "ElectricField" : null;
            if (replacement == null) return;
            SimVector2 position = world.Entities[field.EntityId].Position;
            world.Destroy(field.EntityId);
            world.Spawn(replacement, 0, position);
            field.RemainingFrames = 0;
        }

        private void TickFields()
        {
            for (int index = fieldInstances.Count - 1; index >= 0; index--)
            {
                FieldInstance field = fieldInstances[index];
                if (world.Entities[field.EntityId].IsDestroyed || --field.RemainingFrames <= 0)
                {
                    world.Destroy(field.EntityId);
                    fieldInstances.RemoveAt(index);
                }
            }
        }

        private FieldInstance FindField(int entityId)
        {
            for (int index = 0; index < fieldInstances.Count; index++)
                if (fieldInstances[index].EntityId == entityId) return fieldInstances[index];
            return null;
        }

        private static int StatusDuration(string statusId)
        {
            switch (statusId)
            {
                case "shock": return 10;
                case "burn":
                case "wet":
                case "snared": return 60;
                default: return 0;
            }
        }

        private void ApplyProjectileHits()
        {
            for (int eventIndex = 0; eventIndex < world.CollisionEvents.Count; eventIndex++)
            {
                SimulationCollisionEvent collision = world.CollisionEvents[eventIndex];
                if (collision.Type != SimulationCollisionEventType.Enter) continue;
                HitProjectile(collision.EntityA, collision.EntityB);
                HitProjectile(collision.EntityB, collision.EntityA);
                CaptureTornadoVictim(collision.EntityA, collision.EntityB);
                CaptureTornadoVictim(collision.EntityB, collision.EntityA);
                ApplyPersistentAreaCollision(collision.EntityA, collision.EntityB);
                ApplyPersistentAreaCollision(collision.EntityB, collision.EntityA);
                ApplyExplosionEffectCollision(collision.EntityA, collision.EntityB);
                ApplyExplosionEffectCollision(collision.EntityB, collision.EntityA);
            }
        }

        private void ApplyDropCollisions()
        {
            for (int index = 0; index < world.CollisionEvents.Count; index++)
            {
                SimulationCollisionEvent collision = world.CollisionEvents[index];
                if (collision.Type != SimulationCollisionEventType.Enter) continue;
                ActivateDropOnCollision(collision.EntityA, collision.EntityB);
                ActivateDropOnCollision(collision.EntityB, collision.EntityA);
            }
            for (int index = 0; index < instances.Count; index++)
            {
                Instance instance = instances[index];
                if (!IsCollisionActivatedDrop(instance.Definition.ExecutionKind) ||
                    instance.ActivationRemainingFrames <= 0 ||
                    world.Entities[instance.EntityId].IsDestroyed) continue;
                SimulationEntity source = world.Entities[instance.EntityId];
                foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
                {
                    SimulationEntity target = world.Entities[pair.Key];
                    if (target.IsDestroyed) continue;
                    Fix64 dx = target.Position.X - source.Position.X;
                    Fix64 dy = target.Position.Y - source.Position.Y;
                    Fix64 dz = target.Position3D.Z - source.Position3D.Z;
                    Fix64 overlap = source.Radius + target.Radius;
                    if (dx * dx + dy * dy + dz * dz >= overlap * overlap) continue;
                    ActivateDropOnCollision(instance.EntityId, target.Id);
                    if (source.IsDestroyed) break;
                }
            }
        }

        private void ActivateDropOnCollision(int dropId, int otherId)
        {
            Instance instance = FindInstance(dropId);
            if (instance == null || instance.ActivationRemainingFrames <= 0 ||
                !IsCollisionActivatedDrop(instance.Definition.ExecutionKind)) return;
            if (instance.Definition.ExecutionKind == MagicExecutionKind.DropImpact)
            {
                if (!combatants.TryGetValue(otherId, out MagicCombatState target) ||
                    world.Entities[otherId].OwnerUserId == instance.OwnerUserId) return;
                Damage(otherId, target, instance.Definition.Damage,
                    world.Entities[dropId].Elements);
            }
            else if (!ActivateDrop(instance)) return;
            instance.ActivationRemainingFrames = 0;
            instance.RemainingFrames = 0;
            world.Destroy(dropId);
        }

        private void ApplyPersistentAreaCollision(int sourceId, int targetId)
        {
            Instance source = FindInstance(sourceId);
            if (source == null || source.Definition.ExecutionKind != MagicExecutionKind.PersistentArea ||
                !combatants.TryGetValue(targetId, out MagicCombatState target)) return;
            ApplyStatusToTarget(targetId, target, source.Definition.StatusId,
                source.Definition.StatusFrames);
        }

        private void ApplyExplosionEffectCollision(int sourceId, int targetId)
        {
            Instance source = FindInstance(sourceId);
            if (source == null || source.Definition.ExecutionKind != MagicExecutionKind.Explosion ||
                !combatants.TryGetValue(targetId, out MagicCombatState target)) return;
            if (source.Definition.StatusId != "knockback")
            {
                ApplyStatusToTarget(targetId, target, source.Definition.StatusId,
                    source.Definition.StatusFrames);
                return;
            }
            SimulationEntity center = world.Entities[sourceId];
            SimulationEntity entity = world.Entities[targetId];
            SimVector2 direction = Direction(center.Position, entity.Position);
            Fix64 dx = entity.Position.X - center.Position.X;
            Fix64 dy = entity.Position.Y - center.Position.Y;
            Fix64 distance = Fix64.Sqrt(dx * dx + dy * dy);
            ApplyKnockbackToTarget(targetId, target, direction, source.Definition.Radius <= Fix64.Zero
                ? Fix64.One : distance / source.Definition.Radius);
        }

        private void CaptureTornadoVictim(int tornadoId, int targetId)
        {
            Instance tornado = FindInstance(tornadoId);
            if (tornado == null || tornado.Definition.ExecutionKind != MagicExecutionKind.MovingTornado ||
                !combatants.ContainsKey(targetId) || tornado.HitEntityIds.Contains(targetId)) return;
            SimulationEntity center = world.Entities[tornadoId];
            SimulationEntity target = world.Entities[targetId];
            tornado.HitEntityIds.Add(targetId);
            // Preserve the server Tornado component's center-to-victim angle convention.
            SimVector2 offset = new SimVector2(center.Position.X - target.Position.X,
                center.Position.Y - target.Position.Y);
            if (offset.X * offset.X + offset.Y * offset.Y < (Fix64)0.01m)
                offset = new SimVector2((Fix64)0.1m, Fix64.Zero);
            tornado.OrbitOffsets.Add(targetId, offset);
        }

        private void HitProjectile(int projectileId, int targetId)
        {
            Instance instance = FindInstance(projectileId);
            if (instance == null || !IsMovingShot(instance.Definition.ExecutionKind)) return;
            if (!combatants.TryGetValue(targetId, out MagicCombatState target) ||
                world.Entities[targetId].OwnerUserId == instance.OwnerUserId) return;
            switch (instance.Definition.ExecutionKind)
            {
                case MagicExecutionKind.ChainShot: HitChain(instance, targetId, target); break;
                case MagicExecutionKind.RollingRock: HitRollingRock(instance, targetId, target); break;
                case MagicExecutionKind.PushShot: HitPushShot(instance, targetId, target); break;
                case MagicExecutionKind.MindControlShot: HitMindControl(instance, targetId); break;
                case MagicExecutionKind.PiercingShot: HitPiercing(instance, targetId, target); break;
                default:
                    ApplyStatusToTarget(targetId, target, instance.Definition.StatusId,
                        instance.Definition.StatusFrames);
                    ApplyShotExplosion(instance);
                    world.Destroy(projectileId); instance.RemainingFrames = 0;
                    break;
            }
        }

        private void ApplyShotExplosion(Instance instance)
        {
            SimVector2 center = world.Entities[instance.EntityId].Position;
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                SimulationEntity candidate = world.Entities[pair.Key];
                if (candidate.IsDestroyed || candidate.OwnerUserId == instance.OwnerUserId) continue;
                Fix64 dx = candidate.Position.X - center.X;
                Fix64 dy = candidate.Position.Y - center.Y;
                Fix64 dz = candidate.Position3D.Z - world.Entities[instance.EntityId].Position3D.Z;
                Fix64 overlap = Fix64.One + candidate.Radius;
                if (dx * dx + dy * dy + dz * dz < overlap * overlap)
                    Damage(pair.Key, pair.Value, instance.Definition.Damage,
                        world.Entities[instance.EntityId].Elements);
            }
        }

        private void HitChain(Instance instance, int targetId, MagicCombatState target)
        {
            if (instance.CooldownFrames > 0 || !instance.HitEntityIds.Add(targetId)) return;
            Damage(targetId, target, instance.CurrentDamage,
                world.Entities[instance.EntityId].Elements);
            ApplyStatusToTarget(targetId, target, instance.Definition.StatusId,
                instance.Definition.StatusFrames);
            instance.CurrentDamage = Math.Max(instance.CurrentDamage / 2, instance.Definition.MinimumDamage);
            instance.HitCount++;
            if (instance.HitCount >= instance.Definition.MaximumHits)
            {
                world.Destroy(instance.EntityId); instance.RemainingFrames = 0; return;
            }
            int nextId = FindNextChainTarget(instance, targetId);
            if (nextId < 0)
            {
                world.Destroy(instance.EntityId); instance.RemainingFrames = 0; return;
            }
            SimulationEntity projectile = world.Entities[instance.EntityId];
            projectile.SetPosition(world.Entities[targetId].Position);
            instance.Direction = Direction(projectile.Position, world.Entities[nextId].Position);
            projectile.SetVelocity(SimVector2.Zero);
            instance.PendingTargetId = nextId;
            instance.CooldownFrames = 4;
        }

        private int FindNextChainTarget(Instance instance, int originId)
        {
            SimulationEntity origin = world.Entities[originId];
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                if (instance.HitEntityIds.Contains(pair.Key)) continue;
                SimulationEntity candidate = world.Entities[pair.Key];
                if (candidate.IsDestroyed || candidate.OwnerUserId == instance.OwnerUserId) continue;
                Fix64 dx = candidate.Position.X - origin.Position.X;
                Fix64 dy = candidate.Position.Y - origin.Position.Y;
                Fix64 dz = candidate.Position3D.Z - origin.Position3D.Z;
                Fix64 overlap = instance.Definition.EffectRadius + candidate.Radius;
                if (dx * dx + dy * dy + dz * dz < overlap * overlap) return pair.Key;
            }
            return -1;
        }

        private void HitRollingRock(Instance instance, int targetId, MagicCombatState target)
        {
            Damage(targetId, target, instance.Definition.Damage,
                world.Entities[instance.EntityId].Elements);
            if (instance.BounceCount >= instance.Definition.MaximumHits)
            {
                world.Destroy(instance.EntityId); instance.RemainingFrames = 0; return;
            }
            instance.BounceCount++;
            SimulationEntity projectile = world.Entities[instance.EntityId];
            SimulationEntity other = world.Entities[targetId];
            Fix64 nx = projectile.Position.X - other.Position.X;
            Fix64 ny = projectile.Position.Y - other.Position.Y;
            Fix64 length = Fix64.Sqrt(nx * nx + ny * ny);
            SimVector2 normal = length == Fix64.Zero
                ? new SimVector2(-instance.Direction.X, -instance.Direction.Y)
                : new SimVector2(nx / length, ny / length);
            Fix64 dot = instance.Direction.X * normal.X + instance.Direction.Y * normal.Y;
            instance.Direction = new SimVector2(instance.Direction.X - (Fix64)2 * dot * normal.X,
                instance.Direction.Y - (Fix64)2 * dot * normal.Y);
            projectile.SetVelocity(instance.Direction * instance.Definition.Speed);
        }

        private void HitPushShot(Instance instance, int targetId, MagicCombatState target)
        {
            if (!instance.HitEntityIds.Add(targetId)) return;
            Damage(targetId, target, instance.Definition.Damage,
                world.Entities[instance.EntityId].Elements);
            ApplyStatusToTarget(targetId, target, instance.Definition.StatusId,
                instance.Definition.StatusFrames);
        }

        private void HitMindControl(Instance instance, int targetId)
        {
            SimulationEntity target = world.Entities[targetId];
            if (!IsMindControllable(target)) return;
            target.SetOwner(instance.OwnerUserId);
            world.Destroy(instance.EntityId); instance.RemainingFrames = 0;
        }

        private void HitPiercing(Instance instance, int targetId, MagicCombatState target)
        {
            if (!instance.HitEntityIds.Add(targetId)) return;
            Damage(targetId, target, instance.CurrentDamage,
                world.Entities[instance.EntityId].Elements);
            if (instance.Definition.StatusId == "knockback")
            {
                SimulationEntity projectile = world.Entities[instance.EntityId];
                SimulationEntity targetEntity = world.Entities[targetId];
                Fix64 dx = projectile.Position.X - targetEntity.Position.X;
                Fix64 dy = projectile.Position.Y - targetEntity.Position.Y;
                Fix64 dz = projectile.Position3D.Z - targetEntity.Position3D.Z;
                Fix64 distance = Fix64.Sqrt(dx * dx + dy * dy + dz * dz);
                Fix64 proximity = instance.Definition.Radius <= Fix64.Zero
                    ? Fix64.One : distance / instance.Definition.Radius;
                ApplyKnockbackToTarget(targetId, target, instance.Direction, proximity);
            }
            else
                ApplyStatusToTarget(targetId, target, instance.Definition.StatusId,
                    instance.Definition.StatusFrames);
            instance.CurrentDamage = instance.CurrentDamage * 2 / 3;
        }

        private void ApplyKnockbackMovement()
        {
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                MagicCombatState state = pair.Value;
                if (state.KnockbackFrames <= 0 || world.Entities[pair.Key].IsDestroyed) continue;
                Fix64 step = (Fix64)0.2m * state.KnockbackProximity;
                SimulationEntity entity = world.Entities[pair.Key];
                if (entity.IsStatic ||
                    !entity.HasComponent(SimulationPrefabRegistry.RigidBodyComponentId)) continue;
                entity.SetPosition(new SimVector2(entity.Position.X + state.KnockbackDirection.X * step,
                    entity.Position.Y + state.KnockbackDirection.Y * step));
                if (state.KnockbackFrames == 1 &&
                    entity.HasComponent(SimulationPrefabRegistry.ZPhysicsComponentId))
                    entity.AddVerticalImpulse((Fix64)(-10) * state.KnockbackProximity);
            }
        }

        private void ApplyKnockbackToTarget(int entityId, MagicCombatState target,
            SimVector2 direction, Fix64 proximity)
        {
            if (target.KnockbackFrames > 0) return;
            Fix64 effective = proximity < (Fix64)0.2m ? (Fix64)0.2m : proximity;
            if (world.Entities[entityId].HasComponent(SimulationPrefabRegistry.ZPhysicsComponentId))
                world.Entities[entityId].AddVerticalImpulse((Fix64)10 * effective);
            target.ApplyKnockback(direction, effective);
        }

        private void TickInstances()
        {
            instances.Sort((left, right) => left.EntityId.CompareTo(right.EntityId));
            for (int index = instances.Count - 1; index >= 0; index--)
            {
                Instance instance = instances[index];
                if (world.Entities[instance.EntityId].IsDestroyed)
                {
                    instances.RemoveAt(index);
                    continue;
                }
                TickMovingInstance(instance);
                if (instance.Definition.ExecutionKind == MagicExecutionKind.LeafairDrop &&
                    ApplyFirstImpact(instance, true))
                {
                    world.Destroy(instance.EntityId);
                    instances.RemoveAt(index);
                    continue;
                }
                if (instance.Definition.ExecutionKind == MagicExecutionKind.MovingTornado)
                    TickTornadoVictims(instance);
                if (world.Entities[instance.EntityId].IsDestroyed)
                {
                    instances.RemoveAt(index);
                    continue;
                }
                if (instance.RemainingFrames < 0) continue;
                if (instance.ActivationRemainingFrames > 0)
                {
                    instance.ActivationRemainingFrames--;
                    if (instance.ActivationRemainingFrames == 0)
                    {
                        if (instance.Definition.ExecutionKind == MagicExecutionKind.RallyingTorch)
                        {
                            SimulationEntity landed = world.Entities[instance.EntityId];
                            landed.SetPosition3D(new SimVector3(
                                landed.Position.X, landed.Position.Y, Fix64.Zero));
                            landed.SetVerticalVelocity(Fix64.Zero);
                        }
                        if (ActivateDropAtFallEnd(instance))
                        {
                            world.Destroy(instance.EntityId);
                            instances.RemoveAt(index);
                            continue;
                        }
                    }
                }
                else if (instance.Definition.ExecutionKind == MagicExecutionKind.RallyingTorch)
                    ApplyRallyingTorch(instance);
                else if (instance.Definition.ExecutionKind == MagicExecutionKind.MeteorShower)
                    TickMeteorShower(instance);
                else if (instance.Definition.ExecutionKind == MagicExecutionKind.PeriodicArea ||
                    instance.Definition.ExecutionKind == MagicExecutionKind.MovingTornado)
                    TickPeriodicArea(instance);
                instance.RemainingFrames--;
                if (instance.RemainingFrames > 0) continue;
                if (instance.Definition.ExecutionKind == MagicExecutionKind.Explosion)
                    ApplyArea(instance.OwnerUserId, world.Entities[instance.EntityId].Position,
                        instance.Definition);
                else if (instance.Definition.ExecutionKind == MagicExecutionKind.Overgrowth)
                    ResolveOvergrowth(instance);
                world.Destroy(instance.EntityId);
                instances.RemoveAt(index);
            }
        }

        private bool ActivateDrop(Instance instance)
        {
            switch (instance.Definition.ExecutionKind)
            {
                case MagicExecutionKind.DropImpact:
                    ApplyFirstImpact(instance, false);
                    return true;
                case MagicExecutionKind.LeafairDrop:
                    ApplyFirstImpact(instance, true);
                    return true;
                case MagicExecutionKind.FrenzyDrop:
                    ApplyStatusArea(instance, false);
                    return true;
                case MagicExecutionKind.RallyingTorch:
                    ApplyStatusArea(instance, true);
                    return false;
                default:
                    return false;
            }
        }

        private bool ActivateDropAtFallEnd(Instance instance)
        {
            switch (instance.Definition.ExecutionKind)
            {
                case MagicExecutionKind.FrenzyDrop:
                    ApplyStatusArea(instance, false);
                    return true;
                case MagicExecutionKind.DropImpact:
                case MagicExecutionKind.LeafairDrop:
                    return true;
                case MagicExecutionKind.RallyingTorch:
                    ApplyStatusArea(instance, true);
                    return false;
                default:
                    return false;
            }
        }

        private bool ApplyFirstImpact(Instance instance, bool healAlly)
        {
            SimulationEntity source = world.Entities[instance.EntityId];
            Fix64 radius = instance.Definition.EffectRadius > Fix64.Zero
                ? instance.Definition.EffectRadius : instance.Definition.Radius;
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                SimulationEntity target = world.Entities[pair.Key];
                if (target.IsDestroyed) continue;
                SimVector2 delta = new SimVector2(target.Position.X - source.Position.X,
                    target.Position.Y - source.Position.Y);
                Fix64 overlap = radius + target.Radius;
                Fix64 dz = target.Position3D.Z - source.Position3D.Z;
                if (delta.X * delta.X + delta.Y * delta.Y + dz * dz >= overlap * overlap) continue;
                if (target.OwnerUserId == instance.OwnerUserId)
                {
                    if (!healAlly) continue;
                    pair.Value.Heal(instance.Definition.Damage);
                    pair.Value.RecoverLifetime();
                    return true;
                }
                Damage(pair.Key, pair.Value, instance.Definition.Damage,
                    world.Entities[instance.EntityId].Elements);
                ApplyStatusToTarget(pair.Key, pair.Value, instance.Definition.StatusId,
                    instance.Definition.StatusFrames);
                return true;
            }
            return false;
        }

        private void ApplyStatusArea(Instance instance, bool alliesOnly)
        {
            SimulationEntity source = world.Entities[instance.EntityId];
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                SimulationEntity target = world.Entities[pair.Key];
                if (target.IsDestroyed || alliesOnly && target.OwnerUserId != instance.OwnerUserId) continue;
                SimVector2 delta = new SimVector2(target.Position.X - source.Position.X,
                    target.Position.Y - source.Position.Y);
                Fix64 overlap = instance.Definition.EffectRadius + target.Radius;
                Fix64 dz = target.Position3D.Z - source.Position3D.Z;
                if (delta.X * delta.X + delta.Y * delta.Y + dz * dz >= overlap * overlap) continue;
                ApplyStatusToTarget(pair.Key, pair.Value, instance.Definition.StatusId,
                    instance.Definition.StatusFrames);
            }
        }

        private void ApplyRallyingTorch(Instance instance)
        {
            SimulationEntity source = world.Entities[instance.EntityId];
            foreach (KeyValuePair<int, MagicCombatState> pair in combatants)
            {
                SimulationEntity target = world.Entities[pair.Key];
                if (target.IsDestroyed || target.OwnerUserId != instance.OwnerUserId) continue;
                if (target.HasComponent(SimulationPrefabRegistry.BehaviorMobComponentId))
                    pair.Value.RallyTo(instance.EntityId);
                Fix64 dx = target.Position.X - source.Position.X;
                Fix64 dy = target.Position.Y - source.Position.Y;
                Fix64 dz = target.Position3D.Z - source.Position3D.Z;
                Fix64 overlap = instance.Definition.EffectRadius + target.Radius;
                if (dx * dx + dy * dy + dz * dz < overlap * overlap)
                    ApplyStatusToTarget(pair.Key, pair.Value, instance.Definition.StatusId,
                        instance.Definition.StatusFrames);
            }
        }

        private void ApplyStatusToTarget(int entityId, MagicCombatState target, string statusId, int frames)
        {
            if (string.IsNullOrEmpty(statusId) || frames <= 0) return;
            if (statusId == "shock-overload")
            {
                target.ApplyStatus(statusId, frames);
                return;
            }
            SimulationEntity entity = world.Entities[entityId];
            if (entity.HasComponent(SimulationPrefabRegistry.BuildingEffectReceiverComponentId))
            {
                if (statusId == "snared") { target.ApplyStatus("building-snare-dot", 60); return; }
                if (statusId == "burn" || statusId == "wet" || statusId == "inspired" ||
                    statusId == "knockback")
                    target.ApplyBuildingStatus(statusId, frames);
                return;
            }
            if (!entity.HasComponent(SimulationPrefabRegistry.CommonEffectReceiverComponentId)) return;
            if (IsElementImmune(entity.Elements, statusId)) return;
            if (statusId == "burn" && target.Statuses.ContainsKey("snared"))
                Damage(entityId, target, 5, SimulationElement.Fire);
            target.ApplyStatus(statusId, frames);
            if (statusId == "wet" && (entity.Elements & SimulationElement.Nature) != 0)
                target.ApplyStatus("nature-wet-heal", 60);
        }

        private static bool IsElementImmune(SimulationElement elements, string statusId) =>
            statusId == "burn" && (elements & SimulationElement.Fire) != 0 ||
            statusId == "wet" && (elements & SimulationElement.Water) != 0 ||
            statusId == "shock" && (elements & SimulationElement.Lightning) != 0 ||
            statusId == "snared" && (elements & SimulationElement.Nature) != 0;

        private void Damage(int entityId, MagicCombatState target, int amount,
            SimulationElement attackElements)
        {
            target.Damage(ScaledDamage(entityId, target, amount, attackElements));
        }

        private int ScaledDamage(int entityId, MagicCombatState target, int amount,
            SimulationElement attackElements)
        {
            SimulationElement defense = world.Entities[entityId].Elements;
            if (target.Statuses.ContainsKey("wet")) defense |= SimulationElement.Water;
            int remaining = SimulationElementalChart.ApplyToHealth(
                target.Health, amount, attackElements, defense);
            return target.Health - remaining;
        }

        private void TickMeteorShower(Instance instance)
        {
            instance.EffectElapsedFrames++;
            if (instance.Definition.EffectIntervalFrames <= 0 ||
                instance.EffectElapsedFrames % instance.Definition.EffectIntervalFrames != 0) return;
            DeterministicRandom random = new DeterministicRandom(unchecked(sessionSeed +
                instance.EntityId * 1000003L + instance.EffectElapsedFrames));
            SimVector2 center = world.Entities[instance.EntityId].Position;
            Fix64 radius = instance.Definition.EffectRadius;
            Fix64 x;
            Fix64 y;
            do
            {
                x = (Fix64)(random.NextInt(2001) - 1000) / (Fix64)1000;
                y = (Fix64)random.NextInt(1001) / (Fix64)1000;
            } while (x * x + y * y > Fix64.One);
            SimulationEntity drop = world.Spawn(instance.Definition.SecondaryPrefabId,
                instance.OwnerUserId,
                new SimVector2(center.X + x * radius, center.Y + y * radius));
            StartFalling(drop, 40);
        }

        private void CaptureInstancePositions()
        {
            for (int index = 0; index < instances.Count; index++)
                if (!world.Entities[instances[index].EntityId].IsDestroyed &&
                    IsMovingShot(instances[index].Definition.ExecutionKind))
                    instances[index].PreviousPosition = world.Entities[instances[index].EntityId].Position;
        }

        private void MovePushedTargets()
        {
            for (int index = 0; index < instances.Count; index++)
            {
                Instance instance = instances[index];
                if (instance.Definition.ExecutionKind != MagicExecutionKind.PushShot ||
                    world.Entities[instance.EntityId].IsDestroyed) continue;
                SimVector2 current = world.Entities[instance.EntityId].Position;
                SimVector2 delta = new SimVector2(current.X - instance.PreviousPosition.X,
                    current.Y - instance.PreviousPosition.Y);
                foreach (int targetId in instance.HitEntityIds)
                {
                    SimulationEntity target = world.Entities[targetId];
                    if (!target.IsDestroyed && target.OwnerUserId != instance.OwnerUserId)
                        target.SetPosition(new SimVector2(target.Position.X + delta.X,
                            target.Position.Y + delta.Y));
                }
            }
        }

        private void TickMovingInstance(Instance instance)
        {
            if (!IsMovingShot(instance.Definition.ExecutionKind)) return;
            SimulationEntity entity = world.Entities[instance.EntityId];
            if (instance.Definition.ExecutionKind == MagicExecutionKind.RollingRock &&
                !instance.Direction.Equals(SimVector2.Zero))
            {
                Fix64 minimumX = entity.Radius;
                Fix64 maximumX = (Fix64)18 - entity.Radius;
                Fix64 minimumY = entity.Radius;
                Fix64 maximumY = (Fix64)10 - entity.Radius;
                bool hitX = entity.Position.X <= minimumX || entity.Position.X >= maximumX;
                bool hitY = entity.Position.Y <= minimumY || entity.Position.Y >= maximumY;
                if (hitX || hitY)
                {
                    if (instance.BounceCount >= instance.Definition.MaximumHits)
                    {
                        world.Destroy(instance.EntityId); instance.RemainingFrames = 0; return;
                    }
                    instance.BounceCount++;
                    instance.Direction = new SimVector2(hitX ? -instance.Direction.X : instance.Direction.X,
                        hitY ? -instance.Direction.Y : instance.Direction.Y);
                    Fix64 x = Clamp(entity.Position.X, minimumX, maximumX);
                    Fix64 y = Clamp(entity.Position.Y, minimumY, maximumY);
                    if (hitX) x += instance.Direction.X > Fix64.Zero ? (Fix64)0.05m : (Fix64)(-0.05m);
                    if (hitY) y += instance.Direction.Y > Fix64.Zero ? (Fix64)0.05m : (Fix64)(-0.05m);
                    entity.SetPosition(new SimVector2(Clamp(x, minimumX, maximumX),
                        Clamp(y, minimumY, maximumY)));
                    entity.SetVelocity(instance.Direction * instance.Definition.Speed);
                }
            }
            if (instance.CooldownFrames > 0)
            {
                instance.CooldownFrames--;
                if (instance.CooldownFrames == 0)
                {
                    int pendingTargetId = instance.PendingTargetId;
                    instance.PendingTargetId = -1;
                    if (pendingTargetId >= 0 &&
                        combatants.TryGetValue(pendingTargetId, out MagicCombatState pendingTarget) &&
                        !world.Entities[pendingTargetId].IsDestroyed &&
                        Overlaps(entity, world.Entities[pendingTargetId]))
                        HitChain(instance, pendingTargetId, pendingTarget);
                    else
                        entity.SetVelocity(instance.Direction * instance.Definition.Speed);
                }
            }
            if (instance.Definition.PathSpawnDistance <= Fix64.Zero ||
                string.IsNullOrEmpty(instance.Definition.SecondaryPrefabId)) return;
            Fix64 dx = entity.Position.X - instance.LastPathSpawnPosition.X;
            Fix64 dy = entity.Position.Y - instance.LastPathSpawnPosition.Y;
            Fix64 threshold = instance.Definition.PathSpawnDistance;
            if (dx * dx + dy * dy < threshold * threshold) return;
            world.Spawn(instance.Definition.SecondaryPrefabId, instance.OwnerUserId, entity.Position);
            instance.LastPathSpawnPosition = entity.Position;
        }

        private static bool IsMindControllable(SimulationEntity target)
        {
            switch (target.PrefabId)
            {
                case Objects.SimulationPrefabRegistry.PlayerPrefabId:
                case "GroundCannon":
                case "ManaWell":
                case "HealingTotem":
                case "LifeTree":
                case "RockTurret":
                    return false;
                default:
                    return true;
            }
        }

        private void TickPeriodicArea(Instance instance)
        {
            instance.EffectElapsedFrames++;
            if (instance.Definition.EffectIntervalFrames <= 0 ||
                instance.EffectElapsedFrames % instance.Definition.EffectIntervalFrames != 0) return;
            if (instance.Definition.ExecutionKind == MagicExecutionKind.MovingTornado)
            {
                int amount = checked(instance.Definition.Damage + 2 * instance.HitEntityIds.Count);
                foreach (int targetId in instance.HitEntityIds)
                    if (combatants.TryGetValue(targetId, out MagicCombatState target) &&
                        !world.Entities[targetId].IsDestroyed)
                        Damage(targetId, target, amount, world.Entities[instance.EntityId].Elements);
                return;
            }
            ApplyArea(instance.OwnerUserId, world.Entities[instance.EntityId].Position, instance.Definition);
        }

        private void TickTornadoVictims(Instance instance)
        {
            SimulationEntity tornado = world.Entities[instance.EntityId];
            List<int> ids = new List<int>(instance.OrbitOffsets.Keys);
            for (int index = 0; index < ids.Count; index++)
            {
                int targetId = ids[index];
                SimulationEntity target = world.Entities[targetId];
                if (target.IsDestroyed) continue;
                SimVector2 offset = instance.OrbitOffsets[targetId];
                SimVector2 rotated = Rotate(offset, (Fix64)0.9510565163m, (Fix64)(-0.3090169944m));
                instance.OrbitOffsets[targetId] = rotated;
                Fix64 height = target.Position3D.Z + (Fix64)8 * SimulationWorld.FixedDeltaTime;
                if (height > (Fix64)5) height = (Fix64)5;
                target.SetPosition3D(new SimVector3(tornado.Position.X + rotated.X,
                    tornado.Position.Y + rotated.Y, height));
            }
        }

        private void ResolveOvergrowth(Instance instance)
        {
            SimulationEntity source = world.Entities[instance.EntityId];
            int transformed = 0;
            int originalCount = world.Entities.Count;
            for (int index = 0; index < originalCount; index++)
            {
                SimulationEntity target = world.Entities[index];
                if (target.IsDestroyed || target.OwnerUserId != instance.OwnerUserId ||
                    target.PrefabId != "SeedSpirit") continue;
                Fix64 dx = target.Position.X - source.Position.X;
                Fix64 dy = target.Position.Y - source.Position.Y;
                Fix64 dz = target.Position3D.Z - source.Position3D.Z;
                Fix64 overlap = instance.Definition.Radius + target.Radius;
                if (dx * dx + dy * dy + dz * dz >= overlap * overlap) continue;
                SimVector2 position = target.Position;
                world.Destroy(target.Id);
                world.Spawn("TreeGolem", target.OwnerUserId, position);
                transformed++;
            }
            if (transformed > 0) return;
            Fix64 center = (Fix64)(instance.Definition.SecondarySpawnCount - 1) / (Fix64)2;
            for (int index = 0; index < instance.Definition.SecondarySpawnCount; index++)
                world.Spawn(instance.Definition.SecondaryPrefabId, instance.OwnerUserId,
                    new SimVector2(source.Position.X + ((Fix64)index - center) * (Fix64)0.5m,
                        source.Position.Y));
        }

        private void RegisterCombatants()
        {
            for (int index = 0; index < world.Entities.Count; index++)
            {
                SimulationEntity entity = world.Entities[index];
                if (!entity.IsDestroyed && entity.PrefabId == Objects.SimulationPrefabRegistry.PlayerPrefabId &&
                    !combatants.ContainsKey(entity.Id)) combatants.Add(entity.Id, new MagicCombatState(entity.Id, 100));
            }
        }

        private SimulationEntity FindCaster(long userId)
        {
            for (int index = 0; index < world.Entities.Count; index++)
            {
                SimulationEntity entity = world.Entities[index];
                if (!entity.IsDestroyed && entity.OwnerUserId == userId &&
                    entity.PrefabId == Objects.SimulationPrefabRegistry.PlayerPrefabId) return entity;
            }
            throw new InvalidOperationException("Magic caster not found: " + userId);
        }

        private SimulationEntity FindCaster(int entityId, long userId)
        {
            if (entityId < 0 || entityId >= world.Entities.Count)
                throw new InvalidOperationException("Magic caster entity not found: " + entityId);
            SimulationEntity entity = world.Entities[entityId];
            if (entity.IsDestroyed || entity.OwnerUserId != userId)
                throw new InvalidOperationException("Invalid magic caster entity: " + entityId);
            return entity;
        }

        private Instance FindInstance(int entityId)
        {
            for (int index = 0; index < instances.Count; index++)
                if (instances[index].EntityId == entityId) return instances[index];
            return null;
        }

        private long DeriveSeed(MagicCastCommand command, string magicName)
        {
            ulong hash = 14695981039346656037UL;
            hash = Mix(hash, unchecked((ulong)sessionSeed)); hash = Mix(hash, unchecked((ulong)command.UserId));
            hash = Mix(hash, unchecked((uint)command.RequestId));
            for (int index = 0; index < magicName.Length; index++) hash = Mix(hash, magicName[index]);
            return unchecked((long)hash);
        }

        private static ulong Mix(ulong hash, ulong value)
        {
            for (int index = 0; index < 8; index++) hash = unchecked((hash ^ (byte)(value >> (index * 8))) * 1099511628211UL);
            return hash;
        }

        private static SimVector2 AddStableOffset(SimVector2 value, DeterministicRandom random, bool enabled)
        {
            if (!enabled) return value;
            Fix64 x = (Fix64)(random.NextInt(2001) - 1000) / (Fix64)1000;
            random.NextInt(2001); // Server's second offset is vertical and settles back to ground.
            return new SimVector2(value.X + x, value.Y);
        }

        private static SimVector2 Direction(SimVector2 origin, SimVector2 target)
        {
            Fix64 x = target.X - origin.X; Fix64 y = target.Y - origin.Y;
            Fix64 length = Fix64.Sqrt(x * x + y * y);
            return length == Fix64.Zero ? SimVector2.Zero : new SimVector2(x / length, y / length);
        }

        private static bool Overlaps(SimulationEntity left, SimulationEntity right)
        {
            Fix64 x = right.Position.X - left.Position.X;
            Fix64 y = right.Position.Y - left.Position.Y;
            Fix64 radius = left.Radius + right.Radius;
            return x * x + y * y < radius * radius;
        }

        private static SimVector2 Rotate(SimVector2 direction, Fix64 cosine, Fix64 sine) =>
            new SimVector2(direction.X * cosine - direction.Y * sine,
                direction.X * sine + direction.Y * cosine);

        private static void StartFalling(SimulationEntity entity, int fallFrames)
        {
            if (fallFrames <= 0) return;
            entity.SetPosition3D(new SimVector3(entity.Position.X, entity.Position.Y, (Fix64)10));
            entity.SetVerticalVelocity((Fix64)(-10) /
                (fallFrames * SimulationWorld.FixedDeltaTime));
        }

        private static SimVector2 ClampTarget(SimVector2 origin, SimVector2 target, Fix64 range)
        {
            Fix64 x = Clamp(target.X, Fix64.Zero, (Fix64)18);
            Fix64 y = Clamp(target.Y, Fix64.Zero, (Fix64)10);
            SimVector2 bounded = new SimVector2(x, y);
            SimVector2 direction = Direction(origin, bounded);
            Fix64 dx = bounded.X - origin.X; Fix64 dy = bounded.Y - origin.Y;
            Fix64 distance = Fix64.Sqrt(dx * dx + dy * dy);
            return distance <= range ? bounded : origin + direction * range;
        }

        private static Fix64 Clamp(Fix64 value, Fix64 minimum, Fix64 maximum)
        {
            if (value < minimum) return minimum;
            return value > maximum ? maximum : value;
        }

        private static int CeilingToInt(Fix64 value)
        {
            int truncated = (int)value;
            return value > (Fix64)truncated ? checked(truncated + 1) : truncated;
        }

        private static bool IsMovingShot(MagicExecutionKind executionKind) =>
            executionKind == MagicExecutionKind.Shot ||
            executionKind == MagicExecutionKind.ChainShot ||
            executionKind == MagicExecutionKind.RollingRock ||
            executionKind == MagicExecutionKind.PushShot ||
            executionKind == MagicExecutionKind.MindControlShot ||
            executionKind == MagicExecutionKind.PiercingShot;

        private static bool IsFallingMagic(MagicExecutionKind executionKind) =>
            executionKind == MagicExecutionKind.DropImpact ||
            executionKind == MagicExecutionKind.FrenzyDrop ||
            executionKind == MagicExecutionKind.LeafairDrop ||
            executionKind == MagicExecutionKind.RallyingTorch;

        private static bool IsCollisionActivatedDrop(MagicExecutionKind executionKind) =>
            executionKind == MagicExecutionKind.DropImpact ||
            executionKind == MagicExecutionKind.FrenzyDrop;

        private static bool AffectsAllies(MagicDefinition definition) =>
            definition.ExecutionKind == MagicExecutionKind.Explosion ||
            definition.ExecutionKind == MagicExecutionKind.PeriodicArea ||
            definition.ExecutionKind == MagicExecutionKind.MovingTornado;
    }
}

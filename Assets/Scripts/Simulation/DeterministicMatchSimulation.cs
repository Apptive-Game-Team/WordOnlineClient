using System;
using System.Collections.Generic;
using GameScene.Simulation.Core;
using GameScene.Simulation.Magic;
using GameScene.Simulation.Mob;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Pve;
using GameScene.Simulation.Resources;

namespace GameScene.Simulation
{
    public sealed class DeterministicMatchSimulation
    {
        private sealed class PlayerAttackAura
        {
            public string Effect;
            public int RemainingFrames;
        }

        private readonly ResourceSimulation resources;
        private readonly MagicSimulation magic;
        private readonly MobSimulation mobs;
        private readonly IMobDefinitionResolver mobDefinitions;
        private readonly PveSessionSimulation pve;
        private readonly SortedDictionary<long, PlayerAttackAura> playerAttackAuras =
            new SortedDictionary<long, PlayerAttackAura>();
        private readonly SortedDictionary<long, List<string>> playerIdleAuras =
            new SortedDictionary<long, List<string>>();

        public SimulationWorld World => magic.World;
        public ResourceSimulation Resources => resources;
        public MagicSimulation Magic => magic;
        public MobSimulation Mobs => mobs;
        public PveSessionSimulation Pve => pve;
        public SimulationResult Result
        {
            get
            {
                if (pve?.State == PveSessionState.Won) return SimulationResult.LeftWin;
                if (pve?.State == PveSessionState.Lost) return SimulationResult.RightWin;
                return resources.Result;
            }
        }

        public DeterministicMatchSimulation(ResourceSimulation resources, MagicSimulation magic,
            MobSimulation mobs = null, PveSessionSimulation pve = null,
            IMobDefinitionResolver mobDefinitions = null)
        {
            this.resources = resources ?? throw new ArgumentNullException(nameof(resources));
            this.magic = magic ?? throw new ArgumentNullException(nameof(magic));
            this.mobs = mobs;
            this.pve = pve;
            this.mobDefinitions = mobDefinitions;
            if (mobDefinitions != null && mobs == null)
                throw new InvalidOperationException("Mob definitions require a mob simulation");
            if (mobs != null && !ReferenceEquals(mobs.World, magic.World))
                throw new InvalidOperationException("Mob and magic simulations must share one world");
            if (pve != null && !ReferenceEquals(pve.World, magic.World))
                throw new InvalidOperationException("PVE and magic simulations must share one world");
            mobs?.ConfigurePlayerSides(resources.Left.UserId, resources.Right.UserId);
            if (mobs != null && mobDefinitions != null)
            {
                mobs.RegisterWorldEntities(mobDefinitions);
                RegisterMobCombatants();
            }
        }

        public void Step(ConfirmedFrameMessage frame, IReadOnlyList<MagicCastCommand> requestedCasts)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (requestedCasts == null) throw new ArgumentNullException(nameof(requestedCasts));
            if (pve != null && (pve.State == PveSessionState.Won || pve.State == PveSessionState.Lost))
                throw new InvalidOperationException("PVE match already ended");

            if (mobs != null)
            {
                resources.SetManaChargePercent(resources.Left.UserId,
                    mobs.GetManaChargePercent(resources.Left.UserId));
                resources.SetManaChargePercent(resources.Right.UserId,
                    mobs.GetManaChargePercent(resources.Right.UserId));
            }
            TickPlayerAttackAuras();
            ApplyPlayerIdleAuras(frame);
            resources.Step(frame);
            ApplyPlayerAttackAuras(frame);
            IReadOnlyList<MagicCastCommand> acceptedCasts = FilterAcceptedCasts(requestedCasts);
            IReadOnlyList<SimulationInput> movement = mobs != null
                ? mobs.PrepareFrameInputs() : Array.Empty<SimulationInput>();
            SynchronizeMobShieldsToMagic();
            List<MagicCastCommand> allCasts = new List<MagicCastCommand>(acceptedCasts);
            if (mobs != null)
                for (int index = 0; index < mobs.GeneratedMagicCasts.Count; index++)
                    allCasts.Add(mobs.GeneratedMagicCasts[index]);
            magic.Step(allCasts, movement);
            SynchronizeMagicDamageToMobs();
            mobs?.ResolveAfterPhysics();
            if (mobs != null)
                for (int index = 0; index < mobs.GeneratedStatusEffects.Count; index++)
                {
                    MobStatusEvent effect = mobs.GeneratedStatusEffects[index];
                    magic.ApplyExternalStatus(effect.TargetEntityId, effect.StatusId, 60);
                }
            if (mobs != null && mobDefinitions != null)
            {
                mobs.RegisterSpawned(magic.SpawnedThisFrame, mobDefinitions);
                mobs.RegisterWorldEntities(mobDefinitions);
                RegisterMobCombatants();
            }
            SynchronizeMobHealthToMagic();
            ResolvePlayerHealth();
            pve?.ResolveAfterWorldStep(resources.Left.Health <= 0);
        }

        public ulong CalculateStateHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter();
            writer.WriteUInt64(magic.CalculateStateHash());
            writer.WriteUInt64(resources.CalculateStateHash());
            writer.WriteBoolean(mobs != null);
            if (mobs != null) writer.WriteUInt64(mobs.CalculateStateHash());
            writer.WriteBoolean(pve != null);
            if (pve != null) writer.WriteUInt64(pve.CalculateStateHash());
            writer.WriteInt32(playerAttackAuras.Count);
            foreach (KeyValuePair<long, PlayerAttackAura> pair in playerAttackAuras)
            {
                writer.WriteInt64(pair.Key);
                writer.WriteString(pair.Value.Effect);
                writer.WriteInt32(pair.Value.RemainingFrames);
            }
            writer.WriteInt32(playerIdleAuras.Count);
            foreach (KeyValuePair<long, List<string>> pair in playerIdleAuras)
            {
                writer.WriteInt64(pair.Key);
                writer.WriteInt32(pair.Value.Count);
                for (int index = 0; index < pair.Value.Count; index++) writer.WriteString(pair.Value[index]);
            }
            return writer.Hash;
        }

        public SimulationSnapshot CreateSnapshot()
        {
            SortedDictionary<int, SimulationEntityPresentation> states =
                new SortedDictionary<int, SimulationEntityPresentation>();
            for (int index = 0; index < World.Entities.Count; index++)
            {
                SimulationEntity entity = World.Entities[index];
                string status = EntityStatus(entity);
                List<string> effects = EntityEffects(entity.Id);
                List<SimulationGaugeSnapshot> gauges = new List<SimulationGaugeSnapshot>();
                if (mobs != null && mobs.Agents.TryGetValue(entity.Id, out MobAgentState mob))
                {
                    gauges.Add(new SimulationGaugeSnapshot(mob.Health,
                        mob.Definition.MaxHealth, "HP"));
                    if (mob.Definition.LifetimeFrames > 0)
                        gauges.Add(new SimulationGaugeSnapshot(
                            Math.Max(0, mob.ExpireFrame - World.FrameNumber),
                            mob.Definition.LifetimeFrames, "TTL"));
                    if (mob.PanicUntilFrame > World.FrameNumber && !effects.Contains("Panic"))
                        effects.Add("Panic");
                }
                else if (magic.Combatants.TryGetValue(entity.Id, out MagicCombatState combatant))
                    gauges.Add(new SimulationGaugeSnapshot(combatant.Health,
                        combatant.MaxHealth, "HP"));
                states.Add(entity.Id, new SimulationEntityPresentation(status, effects, gauges));
            }
            return World.CreateSnapshot(states);
        }

        private string EntityStatus(SimulationEntity entity)
        {
            if (entity.IsDestroyed) return "Destroyed";
            if (entity.PrefabId == Objects.SimulationPrefabRegistry.PlayerPrefabId)
                for (int index = 0; index < resources.LastCastResults.Count; index++)
                {
                    CastResult result = resources.LastCastResults[index];
                    if (result.UserId != entity.OwnerUserId) continue;
                    if (result.Code == CastResultCode.Success) return "Attack";
                    if (result.Code == CastResultCode.InvalidMagic) return "Hindered";
                }
            if (mobs != null && mobs.Agents.TryGetValue(entity.Id, out MobAgentState mob))
            {
                switch (mob.State)
                {
                    case MobBehaviorState.Attacking: return "Attack";
                    case MobBehaviorState.Chasing:
                    case MobBehaviorState.Floating:
                    case MobBehaviorState.Diving: return "Move";
                    case MobBehaviorState.Dead: return "Destroyed";
                }
            }
            return "Idle";
        }

        private List<string> EntityEffects(int entityId)
        {
            List<string> effects = new List<string>();
            if (!magic.Combatants.TryGetValue(entityId, out MagicCombatState state)) return effects;
            foreach (KeyValuePair<string, int> status in state.Statuses)
            {
                string effect = PresentationEffect(status.Key);
                if (effect != null && !effects.Contains(effect)) effects.Add(effect);
            }
            long owner = World.Entities[entityId].OwnerUserId;
            if (playerAttackAuras.TryGetValue(owner, out PlayerAttackAura aura) &&
                !effects.Contains(aura.Effect)) effects.Add(aura.Effect);
            if (playerIdleAuras.TryGetValue(owner, out List<string> idle))
                for (int index = 0; index < idle.Count; index++)
                    if (!effects.Contains(idle[index])) effects.Add(idle[index]);
            return effects;
        }

        private void ApplyPlayerIdleAuras(ConfirmedFrameMessage frame)
        {
            ConfirmedInputMessage[] inputs = frame.inputs ?? Array.Empty<ConfirmedInputMessage>();
            for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
            {
                ConfirmedInputMessage confirmed = inputs[inputIndex];
                if (confirmed?.input == null || !string.Equals(
                    confirmed.input.type, "setCardSelection", StringComparison.Ordinal)) continue;
                List<string> effects = new List<string>();
                string[] cards = confirmed.input.cards ?? Array.Empty<string>();
                for (int cardIndex = 0; cardIndex < cards.Length; cardIndex++)
                {
                    string effect = IdleAuraEffect(cards[cardIndex]);
                    if (effect != null && !effects.Contains(effect)) effects.Add(effect);
                }
                effects.Sort(StringComparer.Ordinal);
                if (effects.Count == 0) playerIdleAuras.Remove(confirmed.userId);
                else playerIdleAuras[confirmed.userId] = effects;
            }
        }

        private static string IdleAuraEffect(string card)
        {
            switch (card)
            {
                case "Fire": return "FireIdleAura";
                case "Water": return "WaterIdleAura";
                case "Nature": return "NatureIdleAura";
                case "Lightning": return "LightningIdleAura";
                case "Rock": return "RockIdleAura";
                case "Wind": return "WindIdleAura";
                default: return null;
            }
        }

        private void TickPlayerAttackAuras()
        {
            List<long> owners = new List<long>(playerAttackAuras.Keys);
            for (int index = 0; index < owners.Count; index++)
            {
                PlayerAttackAura aura = playerAttackAuras[owners[index]];
                if (--aura.RemainingFrames <= 0) playerAttackAuras.Remove(owners[index]);
            }
        }

        private void ApplyPlayerAttackAuras(ConfirmedFrameMessage frame)
        {
            ConfirmedInputMessage[] inputs = frame.inputs ?? Array.Empty<ConfirmedInputMessage>();
            for (int resultIndex = 0; resultIndex < resources.LastCastResults.Count; resultIndex++)
            {
                CastResult result = resources.LastCastResults[resultIndex];
                if (result.Code != CastResultCode.Success) continue;
                for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
                {
                    ConfirmedInputMessage confirmed = inputs[inputIndex];
                    if (confirmed?.input == null || confirmed.userId != result.UserId ||
                        confirmed.input.id != result.RequestId) continue;
                    string effect = AttackAuraEffect(confirmed.input.cards);
                    if (effect != null)
                        playerAttackAuras[result.UserId] = new PlayerAttackAura
                            { Effect = effect, RemainingFrames = 6 };
                    break;
                }
            }
        }

        private static string AttackAuraEffect(IReadOnlyList<string> cards)
        {
            if (cards == null) return null;
            for (int index = 0; index < cards.Count; index++)
                switch (cards[index])
                {
                    case "Fire": return "FireAttackAura";
                    case "Water": return "WaterAttackAura";
                    case "Nature": return "NatureAttackAura";
                    case "Lightning": return "LightningAttackAura";
                    case "Rock": return "RockAttackAura";
                    case "Wind": return "WindAttackAura";
                }
            return null;
        }

        private static string PresentationEffect(string statusId)
        {
            switch (statusId)
            {
                case "burn": return "Burn";
                case "wet": return "Wet";
                case "shock": case "shock-overload": return "Shock";
                case "snared": return "Snared";
                case "knockback": return "Knockback";
                case "sandstorm": return "Sandstorm";
                case "frenzy": return "Frenzy";
                case "bubble": return "Bubble";
                case "inspired": return "Inspired";
                default: return null;
            }
        }

        private IReadOnlyList<MagicCastCommand> FilterAcceptedCasts(IReadOnlyList<MagicCastCommand> requested)
        {
            HashSet<string> accepted = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < resources.LastCastResults.Count; index++)
            {
                CastResult result = resources.LastCastResults[index];
                if (result.Code == CastResultCode.Success)
                    accepted.Add(InputKey(result.UserId, result.RequestId));
            }

            List<MagicCastCommand> resultCasts = new List<MagicCastCommand>();
            for (int index = 0; index < requested.Count; index++)
            {
                MagicCastCommand cast = requested[index];
                if (accepted.Contains(InputKey(cast.UserId, cast.RequestId))) resultCasts.Add(cast);
            }
            return resultCasts;
        }

        private void ResolvePlayerHealth()
        {
            int leftHealth = resources.Left.Health;
            int rightHealth = resources.Right.Health;
            foreach (KeyValuePair<int, MagicCombatState> pair in magic.Combatants)
            {
                SimulationEntity entity = World.Entities[pair.Key];
                if (entity.OwnerUserId == resources.Left.UserId) leftHealth = pair.Value.Health;
                else if (entity.OwnerUserId == resources.Right.UserId) rightHealth = pair.Value.Health;
            }
            resources.ResolveCombatHealth(leftHealth, rightHealth);
        }

        private void SynchronizeMagicDamageToMobs()
        {
            if (mobs == null) return;
            foreach (KeyValuePair<int, MagicCombatState> pair in magic.Combatants)
                if (mobs.Agents.ContainsKey(pair.Key))
                {
                    mobs.ResolveHealth(pair.Key, pair.Value.Health);
                    mobs.ResolveBubbleShield(pair.Key, pair.Value.BubbleShieldFrames);
                    mobs.ResolveStatuses(pair.Key, pair.Value.Statuses,
                        pair.Value.StatusRevisions);
                    mobs.ResolveRally(pair.Key, pair.Value.RallySourceEntityId);
                    mobs.ResolveLifetimeRecovery(pair.Key, pair.Value.LifetimeRecovered);
                }
        }

        private void SynchronizeMobShieldsToMagic()
        {
            if (mobs == null) return;
            foreach (KeyValuePair<int, MobAgentState> pair in mobs.Agents)
                if (magic.Combatants.ContainsKey(pair.Key))
                    magic.ResolveBubbleShield(pair.Key, pair.Value.BubbleShieldFrames);
        }

        private void SynchronizeMobHealthToMagic()
        {
            if (mobs == null) return;
            foreach (KeyValuePair<int, MobAgentState> pair in mobs.Agents)
                if (magic.Combatants.TryGetValue(pair.Key, out MagicCombatState combatant))
                    combatant.ResolveHealth(pair.Value.Health);
        }

        private void RegisterMobCombatants()
        {
            foreach (KeyValuePair<int, MobAgentState> pair in mobs.Agents)
            {
                if (!magic.Combatants.ContainsKey(pair.Key))
                    magic.RegisterCombatant(pair.Key, pair.Value.Definition.MaxHealth);
            }
        }

        private static string InputKey(long userId, int requestId) =>
            userId.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" +
            requestId.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}

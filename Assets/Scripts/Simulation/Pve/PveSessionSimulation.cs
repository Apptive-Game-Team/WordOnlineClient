using System;
using System.Collections.Generic;
using GameScene.Simulation.Core;
using GameScene.Simulation.Protocol;

namespace GameScene.Simulation.Pve
{
    public enum PveSessionState : byte { Started, Playing, Won, Lost }

    public sealed class PveSessionSimulation
    {
        private readonly SimulationWorld world;
        private readonly PveScenarioDefinition scenario;
        private readonly List<int> objectives = new List<int>();
        private int nextEvent;
        public PveSessionState State { get; private set; } = PveSessionState.Started;
        public SimulationWorld World => world;

        private PveSessionSimulation(SimulationWorld world, PveScenarioDefinition scenario) { this.world = world; this.scenario = scenario; }

        public static PveSessionSimulation Start(LockstepSessionStartMessage start, PveScenarioCatalog catalog)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (!string.Equals(start.sessionType, "PVE", StringComparison.Ordinal)) throw new InvalidOperationException("PVE session type required");
            BootstrapEventMessage scenarioEvent = null; List<BootstrapEventMessage> players = new List<BootstrapEventMessage>();
            foreach (BootstrapEventMessage value in start.bootstrapEvents ?? Array.Empty<BootstrapEventMessage>())
                if (string.Equals(value.type, "START_PVE_SCENARIO", StringComparison.Ordinal)) { if (scenarioEvent != null) throw new InvalidOperationException("Duplicate PVE scenario start"); scenarioEvent = value; }
                else players.Add(value);
            if (scenarioEvent == null) throw new InvalidOperationException("START_PVE_SCENARIO required");
            PveScenarioDefinition definition = catalog.GetRequired(scenarioEvent.scenarioId, start.configVersion);
            SimulationWorld world = new SimulationWorld(start.rngSeed, Objects.SimulationPrefabRegistry.CreateProduction());
            world.ApplyBootstrap(new LockstepSessionStartMessage { leftUserId = start.leftUserId, rightUserId = start.rightUserId, bootstrapEvents = players.ToArray() });
            PveSessionSimulation simulation = new PveSessionSimulation(world, definition); simulation.ApplyEvents(); return simulation;
        }

        public void Step()
        {
            if (State == PveSessionState.Won || State == PveSessionState.Lost) return;
            State = PveSessionState.Playing; world.Step(Array.Empty<SimulationInput>()); ApplyEvents();
            bool anyObjective = false;
            for (int index = 0; index < objectives.Count; index++) if (!world.Entities[objectives[index]].IsDestroyed) { anyObjective = true; break; }
            if (objectives.Count > 0 && !anyObjective) State = PveSessionState.Won;
            else if (world.FrameNumber >= scenario.TimeLimitFrames) State = PveSessionState.Lost;
        }

        public ulong CalculateStateHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter(); writer.WriteUInt64(world.CalculateStateHash()); writer.WriteInt64(scenario.Id);
            writer.WriteString(scenario.ConfigVersion); writer.WriteInt32(nextEvent); writer.WriteInt32((int)State); writer.WriteInt32(objectives.Count);
            for (int index = 0; index < objectives.Count; index++) writer.WriteInt32(objectives[index]); return writer.Hash;
        }

        private void ApplyEvents()
        {
            while (nextEvent < scenario.Events.Count && scenario.Events[nextEvent].Frame == world.FrameNumber)
            {
                PveSpawnEvent value = scenario.Events[nextEvent++]; SimulationEntity entity = world.Spawn(value.PrefabId, value.OwnerUserId, value.Position);
                if (value.IsObjective) objectives.Add(entity.Id);
            }
        }
    }
}

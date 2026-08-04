using System;
using System.Collections.Generic;
using GameScene.Simulation.Core;

namespace GameScene.Simulation.Pve
{
    public readonly struct PveSpawnEvent
    {
        public int Frame { get; }
        public int Sequence { get; }
        public string PrefabId { get; }
        public long OwnerUserId { get; }
        public SimVector2 Position { get; }
        public bool IsObjective { get; }
        public string InstallerId { get; }
        public PveSpawnEvent(int frame, int sequence, string prefabId, long ownerUserId,
            SimVector2 position, bool isObjective = false, string installerId = null)
        { if (frame < 0) throw new ArgumentOutOfRangeException(nameof(frame)); Frame = frame; Sequence = sequence; PrefabId = prefabId; OwnerUserId = ownerUserId; Position = position; IsObjective = isObjective; InstallerId = installerId; }
    }

    public sealed class PveScriptEventDefinition
    {
        private readonly string[] lines;
        public string Id { get; }
        public int TriggerFrame { get; }
        public string SpeakerInstallerId { get; }
        public string MessageKey { get; }
        public IReadOnlyList<string> Lines => lines;
        public PveScriptEventDefinition(string id, int triggerFrame, string speakerInstallerId,
            string messageKey, params string[] lines)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("PVE script ID required", nameof(id));
            if (triggerFrame < 0) throw new ArgumentOutOfRangeException(nameof(triggerFrame));
            if (string.IsNullOrWhiteSpace(messageKey)) throw new ArgumentException("PVE message key required", nameof(messageKey));
            Id = id; TriggerFrame = triggerFrame; SpeakerInstallerId = speakerInstallerId;
            MessageKey = messageKey; this.lines = lines ?? Array.Empty<string>();
        }
    }

    public readonly struct PveScriptEvent
    {
        public string Id { get; }
        public string MessageKey { get; }
        public int SpeakerEntityId { get; }
        public IReadOnlyList<string> Lines { get; }
        public PveScriptEvent(string id, string messageKey, int speakerEntityId,
            IReadOnlyList<string> lines)
        { Id = id; MessageKey = messageKey; SpeakerEntityId = speakerEntityId; Lines = lines; }
    }

    public sealed class PveScenarioDefinition
    {
        private readonly PveSpawnEvent[] events;
        private readonly PveScriptEventDefinition[] scriptEvents;
        public long Id { get; }
        public string ConfigVersion { get; }
        public int TimeLimitFrames { get; }
        public IReadOnlyList<PveSpawnEvent> Events => events;
        public IReadOnlyList<PveScriptEventDefinition> ScriptEvents => scriptEvents;
        public PveScenarioDefinition(long id, string configVersion, int timeLimitFrames,
            IEnumerable<PveSpawnEvent> events,
            IEnumerable<PveScriptEventDefinition> scriptEvents = null)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrEmpty(configVersion)) throw new ArgumentException("Config version required", nameof(configVersion));
            if (timeLimitFrames <= 0) throw new ArgumentOutOfRangeException(nameof(timeLimitFrames));
            Id = id; ConfigVersion = configVersion; TimeLimitFrames = timeLimitFrames;
            this.events = new List<PveSpawnEvent>(events ?? throw new ArgumentNullException(nameof(events))).ToArray();
            Array.Sort(this.events, (left, right) => { int frame = left.Frame.CompareTo(right.Frame); return frame != 0 ? frame : left.Sequence.CompareTo(right.Sequence); });
            for (int index = 1; index < this.events.Length; index++)
                if (this.events[index - 1].Frame == this.events[index].Frame && this.events[index - 1].Sequence == this.events[index].Sequence)
                    throw new InvalidOperationException("Duplicate PVE event sequence at frame " + this.events[index].Frame);
            this.scriptEvents = new List<PveScriptEventDefinition>(scriptEvents ??
                Array.Empty<PveScriptEventDefinition>()).ToArray();
            Array.Sort(this.scriptEvents, (left, right) =>
            {
                int frame = left.TriggerFrame.CompareTo(right.TriggerFrame);
                return frame != 0 ? frame : string.CompareOrdinal(left.Id, right.Id);
            });
            for (int index = 1; index < this.scriptEvents.Length; index++)
                if (string.Equals(this.scriptEvents[index - 1].Id, this.scriptEvents[index].Id,
                    StringComparison.Ordinal))
                    throw new InvalidOperationException("Duplicate PVE script event: " + this.scriptEvents[index].Id);
        }
    }

    public sealed class PveScenarioCatalog
    {
        private readonly SortedDictionary<long, PveScenarioDefinition> definitions = new SortedDictionary<long, PveScenarioDefinition>();
        public PveScenarioCatalog(IEnumerable<PveScenarioDefinition> definitions)
        { foreach (PveScenarioDefinition value in definitions ?? throw new ArgumentNullException(nameof(definitions))) { if (this.definitions.ContainsKey(value.Id)) throw new InvalidOperationException("Duplicate PVE scenario: " + value.Id); this.definitions.Add(value.Id, value); } }
        public PveScenarioDefinition GetRequired(long id, string configVersion)
        {
            if (!definitions.TryGetValue(id, out PveScenarioDefinition value)) throw new InvalidOperationException("Unknown PVE scenario: " + id);
            if (!string.Equals(value.ConfigVersion, configVersion, StringComparison.Ordinal)) throw new InvalidOperationException("PVE config mismatch: " + configVersion);
            return value;
        }
    }
}

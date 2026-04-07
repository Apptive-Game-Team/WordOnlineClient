using System;
using System.Collections.Generic;
using System.Linq;
using GameScene;
using Global;
using Simulation.Core;
using UnityEngine;

namespace Simulation.Bridge
{
    /// <summary>
    /// Drives the lockstep simulation on the client.
    /// Receives sessionStart and confirmedFrame messages from the server,
    /// runs the deterministic SimWorld, and syncs visuals via SimRenderer.
    ///
    /// PVE specifics:
    ///   - Spawns initial objects from sessionStart.initialObjects.
    ///   - Fires scenario dialogue events keyed by frameNum.
    ///   - Detects SimWorld.IsGameOver and reports result to server.
    /// </summary>
    public class LockstepDriver : MonoBehaviour
    {
        public static LockstepDriver Instance { get; private set; }

        private SimWorld _world;
        private SimRenderer _renderer;
        private long _leftUserId;
        private long _rightUserId;
        private bool _initialized;

        // PVE state
        private bool _isPve;
        private bool _resultSent;
        /// <summary>installerId → runtime object ID, populated for PVE sessions.</summary>
        private readonly Dictionary<string, int> _installerIdToObjectId = new();
        /// <summary>frameNum → list of events to fire at that frame, PVE only.</summary>
        private readonly Dictionary<int, List<PveEventDto>> _scenarioEvents = new();

        private void Awake()
        {
            Instance = this;
            _world = new SimWorld();
        }

        private void OnDestroy()
        {
            _renderer?.Dispose();
            Instance = null;
        }

        /// <summary>
        /// Called when a sessionStart message is received from the server.
        /// Initializes the simulation world.
        /// </summary>
        public void HandleSessionStart(string json)
        {
            WDebug.Log("[LockstepDriver] start session");
            
            var dto = JsonUtility.FromJson<SessionStartDto>(json);
            _leftUserId = dto.leftUserId;
            _rightUserId = dto.rightUserId;
            _isPve = dto.sessionType == "PVE";
            _resultSent = false;
            _installerIdToObjectId.Clear();
            _scenarioEvents.Clear();

            // Parse cards
            var leftCards = dto.leftCards.Select(ParseCardType).ToList();
            var rightCards = dto.rightCards.Select(ParseCardType).ToList();

            // Parse parameters from cached game config
            var parameters = ParseParameters();

            // Set up renderer bridge FIRST before initialization to catch all spawn events
            _renderer?.Dispose();
            _renderer = new SimRenderer(_world);

            // Initialize simulation world (this spawns players and wall)
            _world.Init(dto.rngSeed, leftCards, rightCards, parameters);
            WDebug.Log($"[Lockstep] SimWorld initialized: {_world.GetGameObjects().Count} objects");

            // PVE: spawn initial objects then register scenario events
            if (dto.initialObjects != null && dto.initialObjects.Count > 0)
            {
                WDebug.Log($"[Lockstep] Spawning PVE initial objects: {dto.initialObjects.Count}");
                SpawnInitialObjects(dto.initialObjects);
                WDebug.Log($"[Lockstep] Total objects after PVE spawn: {_world.GetGameObjects().Count}");
            }
            
            if (_isPve)
            {
                RegisterScenarioEvents(dto.scenarioEvents);
            }

            _initialized = true;
            WDebug.Log($"[Lockstep] Session initialized: type={dto.sessionType} seed={dto.rngSeed}");
        }

        /// <summary>
        /// Called when a confirmedFrame message is received from the server.
        /// Advances the simulation by one step with the confirmed inputs.
        /// </summary>
        public void HandleConfirmedFrame(string json)
        {
            if (!_initialized)
            {
                WDebug.LogWarning("[Lockstep] Received confirmedFrame before sessionStart");
                return;
            }

            var dto = JsonUtility.FromJson<ConfirmedFrameDto>(json);

            // Convert inputs
            Dictionary<long, SimInputRequest> inputs = null;
            if (dto.inputs != null && dto.inputs.Count > 0)
            {
                inputs = new Dictionary<long, SimInputRequest>();
                foreach (var pi in dto.inputs)
                {
                    inputs[pi.userId] = new SimInputRequest
                    {
                        Cards = pi.cards?.Select(ParseCardType).ToList(),
                        Position = new SimVector3(
                            Fix64.FromDouble(pi.x),
                            Fix64.FromDouble(pi.y),
                            Fix64.FromDouble(pi.z)),
                        Id = pi.id
                    };
                }
            }

            // Step simulation
            _world.Step(inputs, _leftUserId, _rightUserId);

            // Sync frame clock
            FrameClock.SyncTo(dto.frameNum);

            // PVE: fire scenario events for this frame
            if (_isPve)
                CheckScenarioEvents(dto.frameNum);

            // Update UI
            _renderer.UpdateUI();

            // Game over detection
            if (_world.IsGameOver && !_resultSent)
                ReportResult(_world.Loser);
        }

        /// <summary>Get the SimWorld for querying state (e.g., from input handler).</summary>
        public SimWorld World => _world;

        // ── PVE helpers ──────────────────────────────────────────────────────────

        private void SpawnInitialObjects(List<InitialObjectDto> list)
        {
            if (list == null) return;
            foreach (var obj in list)
            {
                if (!Enum.TryParse<PrefabType>(obj.prefabType, out var prefabType))
                {
                    WDebug.LogWarning($"[Lockstep] Unknown PrefabType: {obj.prefabType}");
                    continue;
                }
                if (!Enum.TryParse<Master>(obj.master, out var master))
                {
                    WDebug.LogWarning($"[Lockstep] Unknown Master: {obj.master}");
                    continue;
                }
                var pos = new SimVector3(
                    Fix64.FromDouble(obj.x),
                    Fix64.FromDouble(obj.y),
                    Fix64.FromDouble(obj.z));
                int objectId = _world.SpawnObject(prefabType, master, pos);
                _installerIdToObjectId[obj.installerId] = objectId;
            }
        }

        private void RegisterScenarioEvents(List<PveEventDto> events)
        {
            if (events == null) return;
            foreach (var evt in events)
            {
                if (!_scenarioEvents.TryGetValue(evt.frameNum, out var list))
                    _scenarioEvents[evt.frameNum] = list = new List<PveEventDto>();
                list.Add(evt);
            }
        }

        private void CheckScenarioEvents(int frameNum)
        {
            if (!_scenarioEvents.TryGetValue(frameNum, out var events)) return;
            foreach (var evt in events)
            {
                int speakerObjectId = -1;
                if (!string.IsNullOrEmpty(evt.speakerInstallerId))
                    _installerIdToObjectId.TryGetValue(evt.speakerInstallerId, out speakerObjectId);

                string line = evt.lines != null && evt.lines.Count > 0 ? evt.lines[0] : string.Empty;
                GameScene.Handler.PveDialoguePresenter.ShowLine(speakerObjectId, line);
            }
        }

        private void ReportResult(Master? loser)
        {
            _resultSent = true;
            string loserStr = loser switch
            {
                Master.LeftPlayer  => "LeftPlayer",
                Master.RightPlayer => "RightPlayer",
                _                  => "None"
            };

            var match = SceneContext.MatchInfo;
            if (match == null) return;

            string sessionId = match.sessionId;
            long userId = SceneContext.UserID;
            string destination = $"/app/game/result/{sessionId}/{userId}";
            string payload = $"{{\"loser\":\"{loserStr}\"}}";

            GameScene.StompConnector.Instance?.SendMessageToServer(destination, payload);
            WDebug.Log($"[Lockstep] Result reported: loser={loserStr}");
        }

        // ── Shared helpers ───────────────────────────────────────────────────────

        private static SimCardType ParseCardType(string name)
        {
            if (Enum.TryParse<SimCardType>(name, true, out var result))
                return result;
            WDebug.LogWarning($"[Lockstep] Unknown card type: {name}");
            return SimCardType.Fire; // fallback
        }

        /// <summary>
        /// Builds the nested parameter dictionary from the cached game config.
        /// The config stores parameters as List&lt;GameParameterEntry&gt; with {group, key, value},
        /// which maps to Dictionary&lt;group, Dictionary&lt;key, Fix64&gt;&gt; for SimWorld.
        /// </summary>
        private static Dictionary<string, Dictionary<string, Fix64>> ParseParameters()
        {
            var result = new Dictionary<string, Dictionary<string, Fix64>>();
            var entries = Data.GameConfig.GameDataManager.Config?.parameters;
            if (entries == null) return result;

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.group) || string.IsNullOrEmpty(entry.key))
                    continue;
                if (!result.TryGetValue(entry.group, out var inner))
                    result[entry.group] = inner = new Dictionary<string, Fix64>();
                inner[entry.key] = Fix64.FromDouble(entry.value);
            }

            return result;
        }
    }
}

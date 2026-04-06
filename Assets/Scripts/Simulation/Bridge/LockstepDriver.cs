using System;
using System.Collections.Generic;
using System.Linq;
using Global;
using Simulation.Core;
using UnityEngine;

namespace Simulation.Bridge
{
    /// <summary>
    /// Drives the lockstep simulation on the client.
    /// Receives sessionStart and confirmedFrame messages from the server,
    /// runs the deterministic SimWorld, and syncs visuals via SimRenderer.
    /// </summary>
    public class LockstepDriver : MonoBehaviour
    {
        public static LockstepDriver Instance { get; private set; }

        private SimWorld _world;
        private SimRenderer _renderer;
        private long _leftUserId;
        private long _rightUserId;
        private bool _initialized;

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
            var dto = JsonUtility.FromJson<SessionStartDto>(json);
            _leftUserId = dto.leftUserId;
            _rightUserId = dto.rightUserId;

            // Parse cards
            var leftCards = dto.leftCards.Select(ParseCardType).ToList();
            var rightCards = dto.rightCards.Select(ParseCardType).ToList();

            // Parse parameters (nested dictionary)
            var parameters = ParseParameters(json);

            // Initialize simulation
            _world.Init(dto.rngSeed, leftCards, rightCards, parameters);

            // Set up renderer bridge
            _renderer?.Dispose();
            _renderer = new SimRenderer(_world);

            _initialized = true;
            WDebug.Log($"[Lockstep] Session initialized: seed={dto.rngSeed}, left={_leftUserId}, right={_rightUserId}");
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

            // Update UI
            _renderer.UpdateUI();

            // Check game over
            if (_world.IsGameOver)
            {
                WDebug.Log($"[Lockstep] Game over! Loser: {_world.Loser}");
            }
        }

        /// <summary>
        /// Get the SimWorld for querying state (e.g., from input handler).
        /// </summary>
        public SimWorld World => _world;

        // ── Helpers ──

        private static SimCardType ParseCardType(string name)
        {
            if (Enum.TryParse<SimCardType>(name, true, out var result))
                return result;
            WDebug.LogWarning($"[Lockstep] Unknown card type: {name}");
            return SimCardType.Fire; // fallback
        }

        private static Dictionary<string, Dictionary<string, Fix64>> ParseParameters(string json)
        {
            // JsonUtility cannot handle nested dictionaries natively.
            // Use a simple JSON parser or MiniJSON for the parameters portion.
            // For now, return empty and rely on server-provided defaults.
            // TODO: Implement proper nested JSON parsing for parameters
            var result = new Dictionary<string, Dictionary<string, Fix64>>();

            // Try to extract parameters using simple parsing
            try
            {
                // Use Unity's built-in JSON or a lightweight parser
                // This is a placeholder — in production, use Newtonsoft.Json or similar
                var wrapper = JsonUtility.FromJson<ParametersWrapper>(json);
                if (wrapper?.parameters != null)
                {
                    foreach (var group in wrapper.parameters)
                    {
                        var inner = new Dictionary<string, Fix64>();
                        if (group.values != null)
                        {
                            foreach (var kv in group.values)
                                inner[kv.key] = Fix64.FromDouble(kv.value);
                        }
                        result[group.name] = inner;
                    }
                }
            }
            catch (Exception e)
            {
                WDebug.LogWarning($"[Lockstep] Failed to parse parameters: {e.Message}");
            }

            return result;
        }

        [Serializable]
        private class ParametersWrapper
        {
            public List<ParameterGroup> parameters;
        }

        [Serializable]
        private class ParameterGroup
        {
            public string name;
            public List<ParameterValue> values;
        }

        [Serializable]
        private class ParameterValue
        {
            public string key;
            public double value;
        }
    }
}

using Coach;
using Global.Coach;
using LobbyScene.Button;
using UnityEngine;

namespace LobbyScene.Coach
{
    /// <summary>
    /// The player has been sitting in the lobby doing nothing and is not in a
    /// queue. Nudges them toward a bot match rather than leaving them parked.
    /// </summary>
    public class LobbyIdleRule : ICoachRule
    {
        private PracticeButton practiceButton;
        private Vector3 lastMousePosition;
        private bool hasMouseSample;

        public CoachRuleId Id => CoachRuleId.LobbyIdle;

        public string MessageKey => "coach.lobbyIdle";

        public int Priority => 6;

        public float DwellSeconds => 45f;

        public int MaxShowsPerSession => 2;

        public bool UseAlternatePlacement => true;

        public bool IsActive()
        {
            if (IsMatching())
            {
                return false;
            }

            return !HasInputThisFrame();
        }

        public Transform[] ResolveTargets()
        {
            if (practiceButton == null)
            {
                practiceButton = UnityEngine.Object.FindObjectOfType<PracticeButton>();
            }

            return practiceButton != null ? new[] { practiceButton.transform } : null;
        }

        public void OnShown()
        {
        }

        public void OnHidden()
        {
        }

        private static bool IsMatching()
        {
            LobbySceneViewModel viewModel = LobbySceneViewModel.Instance;
            return viewModel != null
                   && viewModel.CurrentState.Data == LobbySceneViewModel.LobbyState.Matching;
        }

        /// <summary>
        /// Mouse movement counts as activity too. A player who is reading the
        /// deck dropdown is not idle just because they pressed no key.
        /// </summary>
        private bool HasInputThisFrame()
        {
            Vector3 mousePosition = Input.mousePosition;
            bool moved = hasMouseSample && mousePosition != lastMousePosition;

            lastMousePosition = mousePosition;
            hasMouseSample = true;

            return moved || Input.anyKeyDown;
        }
    }
}

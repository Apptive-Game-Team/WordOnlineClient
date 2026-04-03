using Data;
using GameScene.Dto;
using Global;
using LobbyScene.Debugger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Adventures
{
    public class AdventureViewModel : LocalSingletonObject<AdventureViewModel>
    {
        [SerializeField] private AdventureApiService _adventureApi;

        public enum AdventureState
        {
            Idle,
            Requesting,
        }

        public StateEvent<AdventureState> CurrentState = new StateEvent<AdventureState>(AdventureState.Idle);

        public void PlayPVE(long scenarioId)
        {
            WDebug.Log($"Adventure scenario selected: starting PVE session. scenarioId={scenarioId}");
            CurrentState.UpdateData(AdventureState.Requesting);
            StartCoroutine(_adventureApi.RequestPVE(scenarioId, HandleCallback));
        }

        public void PlayPveDebug(long scenarioId)
        {
            WDebug.Log($"Adventure debug requested. scenarioId={scenarioId}");
            CurrentState.UpdateData(AdventureState.Requesting);
            StartCoroutine(_adventureApi.RequestDebugPVE(scenarioId, HandleDebugSessionCreated, HandleDebugFailure));
        }

        private void HandleCallback(string json)
        {
            TypeChecker typeChecker = JsonUtility.FromJson<TypeChecker>(json);
            switch (typeChecker.type)
            {
                case "matchedInfoDto":
                    WDebug.Log("Adventure session matched: transitioning to game scene.");
                    MatchedInfoDto matchedInfoDto = JsonUtility.FromJson<MatchedInfoDto>(json);
                    OnMatched(matchedInfoDto);
                    break;
                case "message":
                    WDebug.Log("Adventure session message received.");
                    SimpleMessageDto messageDto = JsonUtility.FromJson<SimpleMessageDto>(json);
                    if (messageDto.message.Contains("Successfully"))
                    {
                        WDebug.Log("Adventure session request in progress...");
                        CurrentState.UpdateData(AdventureState.Requesting);
                    }
                    else if (messageDto.message.Contains("Failed"))
                    {
                        WDebug.LogWarning("Adventure session request failed.");
                        CurrentState.UpdateData(AdventureState.Idle);
                        SystemMessageUI.Instance.ShowMessage("Failed to start adventure session. Please try again.");
                    }

                    break;
                default:
                    WDebug.LogWarning($"Unknown event type received: {typeChecker.type}");
                    break;
            }
        }

        private void HandleDebugSessionCreated(string json)
        {
            DebugGameResponse response = JsonUtility.FromJson<DebugGameResponse>(json);
            if (response == null || string.IsNullOrWhiteSpace(response.sessionId))
            {
                HandleDebugFailure("Invalid debug session response.");
                return;
            }

            MatchedInfoDto matchedInfoDto = MatchedInfoDto.CreateDebugSession(response.sessionId, "left", SceneContext.UserID);
            OnMatched(matchedInfoDto);
        }

        private void HandleDebugFailure(string message)
        {
            CurrentState.UpdateData(AdventureState.Idle);
            WDebug.LogWarning($"Adventure debug session failed: {message}");
            SystemMessageUI.Instance.ShowMessage("Failed to start adventure debug session. Check the debug server.");
        }

        private void OnMatched(MatchedInfoDto matchedInfoDto)
        {
            SceneContext.MatchInfo = matchedInfoDto;
            string targetSceneName = "GameScene";
            if (SceneManager.GetActiveScene().name.Contains(targetSceneName))
            {
                return;
            }

            SceneManager.LoadScene(targetSceneName);
        }
    }
}

using Data;
using GameScene.Dto;
using Global;
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
            WDebug.Log("Practice button clicked: Starting practice match.");
            CurrentState.UpdateData(AdventureState.Requesting);
            StartCoroutine(_adventureApi.RequestPVE(scenarioId, HandleCallback));
        }

        private void HandleCallback(string json)
        {
            TypeChecker typeChecker = JsonUtility.FromJson<TypeChecker>(json);
            switch (typeChecker.type)
            {
                case "matchedInfoDto":
                    WDebug.Log("Practice match found: Transitioning to game scene.");
                    MatchedInfoDto matchedInfoDto = JsonUtility.FromJson<MatchedInfoDto>(json);
                    OnMatched(matchedInfoDto);
                    break;
                case "message":
                    WDebug.Log("Practice match message received.");
                    SimpleMessageDto messageDto = JsonUtility.FromJson<SimpleMessageDto>(json);
                    if (messageDto.message.Contains("Successfully"))
                    {
                        WDebug.Log("Practice match in progress...");
                        CurrentState.UpdateData(AdventureState.Requesting);
                    }
                    else if (messageDto.message.Contains("Failed"))
                    {
                        WDebug.LogWarning("Practice match failed.");
                        CurrentState.UpdateData(AdventureState.Idle);
                        SystemMessageUI.Instance.ShowMessage("Failed to start practice match. Please try again.");
                    }

                    break;
                default:
                    WDebug.LogWarning($"Unknown event type received: {typeChecker.type}");
                    break;
            }
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
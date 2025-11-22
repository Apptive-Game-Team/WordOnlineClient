using System;
using Script.Data;
using Script.GameScene.Dto;
using Script.Global;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Script.LobbyScene
{
    public class LobbySceneViewModel : LocalSingletonObject<LobbySceneViewModel>
    {
        
        [SerializeField]
        private MatchQueueApiService _matchQueueApi;
        
        public enum LobbyState
        {
            Idle,
            Matching,
        }

        public StateEvent<LobbyState> CurrentState = new StateEvent<LobbyState>(LobbyState.Idle);
        
        public void Enqueue()
        {
            Debug.Log("Enqueue button clicked: Enqueueing player.");
            CurrentState.UpdateData(LobbyState.Matching);
            StartCoroutine(_matchQueueApi.Enqueue(json =>
            {
                TypeChecker typeChecker = JsonUtility.FromJson<TypeChecker>(json);
                switch (typeChecker.type)
                {
                    case "matchedInfoDto":
                        Debug.Log("Practice match found: Transitioning to game scene.");
                        MatchedInfoDto matchedInfoDto = JsonUtility.FromJson<MatchedInfoDto>(json);
                        SceneContext.MatchInfo = matchedInfoDto;
                        SceneManager.LoadScene("GameScene");
                        break;
                    case "message":
                        Debug.Log("Practice match message received.");
                        SimpleMessageDto messageDto = JsonUtility.FromJson<SimpleMessageDto>(json);
                        if (messageDto.message.Contains("Successfully"))
                        {
                            Debug.Log("Practice match in progress...");
                            CurrentState.UpdateData(LobbyState.Matching);
                        }
                        else if (messageDto.message.Contains("Failed"))
                        {
                            Debug.LogWarning("Practice match failed.");
                            CurrentState.UpdateData(LobbyState.Idle);
                        }
                        break;
                    default:
                        Debug.LogWarning($"Unknown event type received: {typeChecker.type}");
                        break;
                }
            }));
        }
        
        public void PlayPracticeMatch()
        {
            Debug.Log("Practice button clicked: Starting practice match.");
            CurrentState.UpdateData(LobbyState.Matching);
            StartCoroutine(_matchQueueApi.MatchPractice(json =>
            {
                TypeChecker typeChecker = JsonUtility.FromJson<TypeChecker>(json);
                switch (typeChecker.type)
                {
                    case "matchedInfoDto":
                        Debug.Log("Practice match found: Transitioning to game scene.");
                        MatchedInfoDto matchedInfoDto = JsonUtility.FromJson<MatchedInfoDto>(json);
                        SceneContext.MatchInfo = matchedInfoDto;
                        SceneManager.LoadScene("GameScene");
                        break;
                    case "message":
                        Debug.Log("Practice match message received.");
                        SimpleMessageDto messageDto = JsonUtility.FromJson<SimpleMessageDto>(json);
                        if (messageDto.message.Contains("Successfully"))
                        {
                            Debug.Log("Practice match in progress...");
                            CurrentState.UpdateData(LobbyState.Matching);
                        }
                        else if (messageDto.message.Contains("Failed"))
                        {
                            Debug.LogWarning("Practice match failed.");
                            CurrentState.UpdateData(LobbyState.Idle);
                        }
                        break;
                    default:
                        Debug.LogWarning($"Unknown event type received: {typeChecker.type}");
                        break;
                }
            }));
        }
        
        public void RemoveFromQueue()
        {
            Debug.Log("Remove button clicked: Removing player.");
            StartCoroutine(_matchQueueApi.RemoveFromQueue());
            CheckIfInQueue();
        }
        
        public void CheckIfInQueue()
        {
            Debug.Log("Check button clicked: Checking player.");
            StartCoroutine(_matchQueueApi.IsMeInQueue(state =>
            {
                CurrentState.UpdateData(state);
            }));
        }
    }
}

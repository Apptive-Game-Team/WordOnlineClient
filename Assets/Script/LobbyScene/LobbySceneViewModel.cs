using System;
using System.Collections;
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
                        OnMatched(matchedInfoDto);
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
                        OnMatched(matchedInfoDto);
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
        
        private float _pollTimer = 0;
        private float POLL_INTERVAL = 6.0f;

        private void Update()
        {
            if (CurrentState.Data == LobbyState.Matching) 
            {
                if (_pollTimer > POLL_INTERVAL)
                {
                    PollMatchedInfo();
                    _pollTimer = 0;
                }
                
                _pollTimer += Time.deltaTime;
            }
        }
        
        public void PollMatchedInfo()
        {
            Debug.Log("Automatic polling: checking match status.");
            StartCoroutine(StatusTracker.GetUserStatus(Handler));
        }

        private IEnumerator Handler(string state)
        {
            switch (state)
            {
                case "Online":
                    CurrentState.UpdateData(LobbyState.Idle);
                    break;
                case "OnPlaying":
                    Debug.Log("User matched: Transitioning to game scene.");
                    yield return StatusTracker.RecoverGameSession();
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

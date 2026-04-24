using System.Collections;
using Data;
using Global;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LobbyScene
{
    public class LobbySceneViewModel : LocalSingletonObject<LobbySceneViewModel>
    {
        [SerializeField] private MatchQueueApiService _matchQueueApi;
        [SerializeField] private MatchStatusPoller _poller;

        public enum LobbyState
        {
            Idle,
            Matching,
        }

        public StateEvent<LobbyState> CurrentState = new StateEvent<LobbyState>(LobbyState.Idle);

        protected override void Awake()
        {
            base.Awake();
            _poller.OnIdle += () => CurrentState.UpdateData(LobbyState.Idle);
            _poller.OnMatched += () => StartCoroutine(StatusTracker.RecoverGameSession());
        }

        public void Enqueue()
        {
            Debug.Log("Enqueue button clicked: Enqueueing player.");
            CurrentState.UpdateData(LobbyState.Matching);
            StartCoroutine(_matchQueueApi.Enqueue(messageDto =>
            {
                if (messageDto == null || messageDto.message.Contains("Failed"))
                {
                    Debug.LogWarning("Enqueue failed.");
                    CurrentState.UpdateData(LobbyState.Idle);
                    return;
                }
                Debug.Log("Enqueued: starting match poller.");
                _poller.StartPolling();
            }));
        }

        public void PlayPracticeMatch()
        {
            Debug.Log("Practice button clicked: Starting practice match.");
            CurrentState.UpdateData(LobbyState.Matching);
            StartCoroutine(_matchQueueApi.MatchPractice(dto =>
            {
                if (dto != null)
                    OnMatched(dto);
                else
                    CurrentState.UpdateData(LobbyState.Idle);
            }));
        }

        private void OnMatched(MatchedInfoDto matchedInfoDto)
        {
            SceneContext.MatchInfo = matchedInfoDto;
            const string targetSceneName = "GameScene";
            if (SceneManager.GetActiveScene().name.Contains(targetSceneName)) return;
            SceneManager.LoadScene(targetSceneName);
        }

        public void RemoveFromQueue()
        {
            Debug.Log("Remove button clicked: Removing player.");
            _poller.StopPolling();
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

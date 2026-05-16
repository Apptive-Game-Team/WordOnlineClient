using System;
using Data;
using Data.Magic;
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

        public StateEvent<LobbyState> CurrentState = new (LobbyState.Idle);
        public event Action OnMatchingStart;
        public event Action OnMatchingStop;
        public event Action OnMatchingFailed;
        public event Action OnMatchingCanceled;
        private bool _isCancelRequested;

        protected override void Awake()
        {
            base.Awake();
            _poller.OnIdle += HandlePolledIdle;
            _poller.OnMatching += () => CurrentState.UpdateData(LobbyState.Matching);
            _poller.OnMatched += () => StartCoroutine(StatusTracker.RecoverGameSession());
        }

        public void Enqueue()
        {
            Debug.Log("Enqueue button clicked: Enqueueing player.");
            StartMatching();
            StartCoroutine(_matchQueueApi.Enqueue(messageDto =>
            {
                if (messageDto == null || messageDto.message.Contains("Failed"))
                {
                    Debug.LogWarning("Enqueue failed.");
                    StopMatchingAsFailed();
                    return;
                }
                Debug.Log("Enqueued: waiting for match.");
            }));
        }

        public void PlayPracticeMatch()
        {
            Debug.Log("Practice button clicked: Starting practice match.");
            StartMatching();
            StartCoroutine(_matchQueueApi.MatchPractice(dto =>
            {
                if (dto != null)
                    OnMatched(dto);
                else
                    StopMatchingAsFailed();
            }));
        }

        private void OnMatched(MatchedInfoDto matchedInfoDto)
        {
            SceneContext.MatchInfo = matchedInfoDto;
            const string targetSceneName = "GameScene";
            if (SceneManager.GetActiveScene().name.Contains(targetSceneName)) return;
            MagicInfoDataSource.Instance.RefreshMagics(_ => SceneManager.LoadScene(targetSceneName));
        }

        public void RemoveFromQueue()
        {
            Debug.Log("Remove button clicked: Removing player.");
            _isCancelRequested = true;
            StartCoroutine(_matchQueueApi.RemoveFromQueue(isSuccess =>
            {
                if (isSuccess) return;

                _isCancelRequested = false;
                CheckIfInQueue();
            }));
        }

        public void CheckIfInQueue()
        {
            Debug.Log("Check button clicked: Checking player.");
            StartCoroutine(_matchQueueApi.IsMeInQueue(state =>
            {
                CurrentState.UpdateData(state);
            }));
        }

        private void StartMatching()
        {
            _isCancelRequested = false;
            CurrentState.UpdateData(LobbyState.Matching);
            OnMatchingStart?.Invoke();
        }

        private void HandlePolledIdle()
        {
            bool wasMatching = CurrentState.Data == LobbyState.Matching;
            CurrentState.UpdateData(LobbyState.Idle);
            if (!wasMatching) return;

            if (_isCancelRequested)
                StopMatchingAsCanceled();
            else
                StopMatchingAsFailed();
        }

        private void StopMatchingAsFailed()
        {
            _isCancelRequested = false;
            CurrentState.UpdateData(LobbyState.Idle);
            OnMatchingFailed?.Invoke();
            OnMatchingStop?.Invoke();
        }

        private void StopMatchingAsCanceled()
        {
            _isCancelRequested = false;
            CurrentState.UpdateData(LobbyState.Idle);
            OnMatchingCanceled?.Invoke();
            OnMatchingStop?.Invoke();
        }
    }
}

using System;
using System.Collections;
using UnityEngine;

namespace LobbyScene
{
    public class MatchStatusPoller : MonoBehaviour
    {
        public event Action OnIdle;
        public event Action OnMatched;

        private bool _active;
        private bool _waiting;
        private float _timer;
        private float _interval;
        private const float MinInterval = 1f;
        private const float MaxInterval = 5f;

        public void StartPolling()
        {
            _active = true;
            _waiting = false;
            _timer = 0f;
            _interval = MinInterval;
        }

        public void StopPolling()
        {
            _active = false;
            _waiting = false;
            _timer = 0f;
            _interval = MinInterval;
        }

        private void Update()
        {
            if (!_active || _waiting) return;

            _timer += Time.deltaTime;
            if (_timer < _interval) return;

            _timer = 0f;
            _interval = Mathf.Min(_interval * 2f, MaxInterval);
            _waiting = true;
            StartCoroutine(StatusTracker.GetUserStatus(HandleStatus));
        }

        private IEnumerator HandleStatus(string state)
        {
            _waiting = false;
            switch (state)
            {
                case "Online":
                    StopPolling();
                    OnIdle?.Invoke();
                    break;
                case "OnPlaying":
                    Debug.Log("User matched: Transitioning to game scene.");
                    StopPolling();
                    OnMatched?.Invoke();
                    break;
            }
            yield break;
        }
    }
}

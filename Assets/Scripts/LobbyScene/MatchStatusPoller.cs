using System;
using System.Collections;
using Global;
using UnityEngine;

namespace LobbyScene
{
    public class MatchStatusPoller : MonoBehaviour
    {
        public event Action OnIdle;
        public event Action OnMatched;
        public event Action OnMatching;

        private bool _waiting;
        private float _timer;
        private bool _prevOnline;

        // The server holds each request until the status changes, so the gap between requests is
        // only there to space out reconnects, not to control how fast a match is noticed.
        private const float Interval = 0.5f;
        private const int LongPollSeconds = 20;

        public void StopPolling()
        {
            _waiting = false;
            _timer = 0f;
            _prevOnline = false;
            enabled = false;
        }

        private void Update()
        {
            if (_waiting) return;

            _timer += Time.deltaTime;
            if (_timer < Interval) return;

            WDebug.Log("[Match Status Poller] Checking user status...");
            _timer = 0f;
            _waiting = true;
            StartCoroutine(StatusTracker.GetUserStatus(HandleStatus, LongPollSeconds));
        }

        private IEnumerator HandleStatus(string state)
        {
            _waiting = false;

            if (state == null)
                yield return null;
            
            switch (state)
            {
                case "Online":
                    if (_prevOnline) OnIdle?.Invoke();
                    else WDebug.Log("[Match Status Poller] Stat: Online - waiting for confirmation.");
                    _prevOnline = true;
                    break;
                case "OnPlaying":
                    _prevOnline = false;
                    Debug.Log("[Match Status Poller] User matched: Transitioning to game scene.");
                    StopPolling();
                    OnMatched?.Invoke();
                    break;
                case "OnMatching":
                    _prevOnline = false;
                    OnMatching?.Invoke();
                    break;
            }
            yield break;
        }
    }
}

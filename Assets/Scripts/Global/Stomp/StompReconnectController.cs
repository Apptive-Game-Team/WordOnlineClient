using System;
using System.Collections;
using UnityEngine;

namespace Global.Stomp
{
    /// <summary>
    /// 지수 백오프(exponential backoff) 방식의 STOMP 재연결 전략.
    /// 연결 실패 시 1s → 2s → 4s → 8s … 대기 후 재시도.
    /// </summary>
    public class StompReconnectController : MonoBehaviour
    {
        [SerializeField] private int maxRetries = 5;
        [SerializeField] private float baseDelay = 1f;

        private int _retryCount;
        private Coroutine _reconnectCoroutine;

        /// <summary>재연결을 시도할 때마다 발생</summary>
        public event Action OnReconnectAttempt;

        /// <summary>최대 재시도 횟수를 초과했을 때 발생</summary>
        public event Action OnMaxRetriesExceeded;

        /// <summary>연결 실패를 알리고 백오프 재연결 루프를 시작</summary>
        public void NotifyConnectionLost()
        {
            if (_reconnectCoroutine != null) return; // 이미 재연결 중
            _reconnectCoroutine = StartCoroutine(ReconnectCoroutine());
        }

        /// <summary>연결 성공 후 상태를 초기화</summary>
        public void ResetRetries()
        {
            _retryCount = 0;
            if (_reconnectCoroutine != null)
            {
                StopCoroutine(_reconnectCoroutine);
                _reconnectCoroutine = null;
            }
        }

        private IEnumerator ReconnectCoroutine()
        {
            while (_retryCount < maxRetries)
            {
                float delay = baseDelay * Mathf.Pow(2f, _retryCount);
                _retryCount++;
                WDebug.Log($"[STOMP] {delay:F1}초 후 재연결 시도 ({_retryCount}/{maxRetries})");
                yield return new WaitForSeconds(delay);
                OnReconnectAttempt?.Invoke();
            }

            _reconnectCoroutine = null;
            WDebug.LogError("[STOMP] 최대 재연결 횟수 초과");
            OnMaxRetriesExceeded?.Invoke();
        }
    }
}

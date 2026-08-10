using System;
using System.Collections;
using UnityEngine;

namespace Global.Stomp
{
    /// <summary>
    /// STOMP 재연결 전략.
    ///
    /// 기본값은 지수 백오프(1s → 2s → 4s → 8s …)다. 다수의 클라이언트가 힘들어하는 서버로
    /// 한꺼번에 몰리는 것을 막기 위한 장치다.
    ///
    /// 인게임처럼 클라이언트 하나가 자기 세션에 다시 붙는 상황은 거기에 해당하지 않고,
    /// 이탈 예산도 몇 초뿐이라 첫 재시도조차 들어가지 않는다. 그런 호출부는
    /// <see cref="UseFixedSchedule"/>로 짧은 고정 간격을 지정한다.
    /// </summary>
    public class StompReconnectController : MonoBehaviour
    {
        [SerializeField] private int maxRetries = 5;
        [SerializeField] private float baseDelay = 1f;

        /// <summary>고정 간격 일정. null이면 지수 백오프를 쓴다.</summary>
        private float[] _fixedDelays;

        private int _retryCount;
        private Coroutine _reconnectCoroutine;

        /// <summary>재연결을 시도할 때마다 발생</summary>
        public event Action OnReconnectAttempt;

        /// <summary>최대 재시도 횟수를 초과했을 때 발생</summary>
        public event Action OnMaxRetriesExceeded;

        /// <summary>실제로 적용 중인 최대 재시도 횟수.</summary>
        public int RetryBudget => _fixedDelays?.Length ?? maxRetries;

        /// <summary>
        /// 지수 백오프 대신 고정 간격 일정을 쓴다. 배열 길이가 곧 최대 재시도 횟수다.
        /// 지정하지 않은 호출부는 인스펙터에 설정된 지수 백오프 그대로 동작한다.
        /// </summary>
        public void UseFixedSchedule(params float[] delaysSeconds)
        {
            _fixedDelays = delaysSeconds != null && delaysSeconds.Length > 0 ? delaysSeconds : null;
        }

        /// <summary>연결 실패를 알리고 재연결 루프를 시작</summary>
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
            int budget = RetryBudget;

            while (_retryCount < budget)
            {
                float delay = DelayFor(_retryCount);
                _retryCount++;
                WDebug.Log($"[STOMP] {delay:F1}초 후 재연결 시도 ({_retryCount}/{budget})");

                // 대기는 실시간 기준이다. 배속이나 일시정지가 재연결 예산을 늘리면 안 된다.
                if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
                else yield return null;

                OnReconnectAttempt?.Invoke();
            }

            _reconnectCoroutine = null;
            WDebug.LogError("[STOMP] 최대 재연결 횟수 초과");
            OnMaxRetriesExceeded?.Invoke();
        }

        private float DelayFor(int retryIndex)
        {
            if (_fixedDelays == null) return baseDelay * Mathf.Pow(2f, retryIndex);
            return _fixedDelays[Mathf.Min(retryIndex, _fixedDelays.Length - 1)];
        }
    }
}

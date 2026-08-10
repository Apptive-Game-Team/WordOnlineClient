using System;
using System.Collections;
using Data;
using Data.Net;
using GameScene.Handler;
using Global;
using Global.Stomp;
using UnityEngine;
using UnityEngine.Localization;

namespace GameScene
{
    /// <summary>
    /// 게임씬 STOMP 오케스트레이터.
    /// 플랫폼별 IStompTransport를 주입받아 연결·구독·재연결을 관리한다.
    ///
    /// 의존 관계:
    ///   StompConnector (게임 로직)
    ///     ├─ IStompTransport      ← WebGLStompTransport | NativeStompTransport
    ///     ├─ StompSubscriptionRegistry  ← 구독 보관 및 재구독
    ///     └─ StompReconnectController   ← 지수 백오프 재연결
    /// </summary>
    public class StompConnector : LocalSingletonObject<StompConnector>
    {
        private const string StompPath = "ws";

        [SerializeField] private bool isSpectator;

        public LocalizedString notConnectedToServer;
        public LocalizedString connectionClosed;
        public LocalizedString connectionDelayed;
        public LocalizedString frameTimeout;

        private IStompTransport _transport;
        private StompSubscriptionRegistry _registry;
        private StompReconnectController _reconnect;

        private readonly IFrameInfoHandler<string> _frameInfoHandler = new GeneralHandler();
        private float _lastFrameTime = -1f;

        /// <summary>이탈 신고가 진행 중인지. 여러 이탈 경로가 동시에 터져도 한 번만 신고한다.</summary>
        private bool _reportingSessionLoss;

        // ─── 생명주기 ────────────────────────────────────────────────────────

        protected override void Awake()
        {
            gameObject.name = "StompConnector";
            base.Awake();

            _registry = new StompSubscriptionRegistry();
            _reconnect = gameObject.AddComponent<StompReconnectController>();

#if UNITY_WEBGL && !UNITY_EDITOR
            _transport = gameObject.AddComponent<WebGLStompTransport>();
#else
            _transport = gameObject.AddComponent<NativeStompTransport>();
#endif

            _transport.Connected += HandleConnected;
            _transport.Disconnected += HandleDisconnected;
            _transport.Errored += HandleError;
            _transport.MessageReceived += _registry.Dispatch;

            _reconnect.OnReconnectAttempt += ConnectToServer;
            _reconnect.OnMaxRetriesExceeded += HandleMaxRetriesExceeded;
        }

        private void Start()
        {
            ConnectToServer();
            StartCoroutine(GameFlowCoroutine(SceneContext.MatchInfo.sessionId));
        }

        private void OnDestroy()
        {
            _transport.Disconnect();
        }

        // ─── 퍼블릭 API ─────────────────────────────────────────────────────

        public void ConnectToServer()
        {
            if (_transport.IsConnected) return;

            MatchedInfoDto matchInfo = SceneContext.MatchInfo;
            if (!matchInfo.TryResolveWebSocket(StompPath, out ServerEndpoint gameServer))
            {
                WDebug.LogError($"[STOMP] 게임 서버 URL을 해석하지 못했습니다: {matchInfo.ConnectionSource}");
                return;
            }

            string url = gameServer.Query("token", SceneContext.JwtToken);
            _transport.Connect(url, SceneContext.JwtToken);
        }

        public void SendMessageToServer(string topic, string message)
        {
            if (!_transport.IsConnected)
            {
                SystemMessageUI.Instance.ShowMessage(notConnectedToServer);
                WDebug.LogError("[STOMP] 미연결 상태에서 메시지 전송 시도: " + topic);
                return;
            }
            _transport.Send(topic, message);
        }

        public void SubscribeToTopic(string topic, Action<string> callback, string subscriptionId)
        {
            if (!_transport.IsConnected)
            {
                SystemMessageUI.Instance.ShowMessage(notConnectedToServer);
                WDebug.LogError("[STOMP] 미연결 상태에서 구독 시도: " + topic);
                return;
            }
            _registry.Register(topic, subscriptionId, callback);
            _transport.Subscribe(topic, subscriptionId);
        }

        public void UnsubscribeFromTopic(string subscriptionId)
        {
            _registry.Unregister(subscriptionId);
            if (_transport.IsConnected)
                _transport.Unsubscribe(subscriptionId);
        }

        // ─── 게임 플로우 ─────────────────────────────────────────────────────

        private IEnumerator GameFlowCoroutine(string sessionId)
        {
            float elapsed = 0f;
            while (!_transport.IsConnected)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= 10f)
                {
                    WDebug.LogError("[STOMP] 연결 타임아웃 (10초)");
                    SystemMessageUI.Instance.ShowMessage(connectionClosed);

                    bool sessionAlive = false;
                    yield return AbandonSession(SessionLossReason.ConnectTimeout, () => sessionAlive = true);
                    if (!sessionAlive) yield break;

                    // 세션이 살아있다는 판정이다. 이탈하지 않고 연결만 다시 시도한다.
                    elapsed = 0f;
                    ConnectToServer();
                }
                yield return null;
            }

            UnsubscribeFromTopic("match-sub");
            long userId = isSpectator ? 0 : SceneContext.UserID;
            SubscribeToTopic($"/game/{sessionId}/frameInfos/{userId}", OnFrameInfoReceived, "frame-sub");

            _lastFrameTime = Time.time;
            while (true)
            {
                if (Time.time - _lastFrameTime >= 10f)
                {
                    WDebug.LogError("[STOMP] FrameInfo 수신 타임아웃 (10초)");
                    SystemMessageUI.Instance.ShowMessage(frameTimeout.IsEmpty ? connectionDelayed : frameTimeout);

                    bool sessionAlive = false;
                    yield return AbandonSession(SessionLossReason.FrameInfoTimeout, () => sessionAlive = true);
                    if (!sessionAlive) yield break;

                    // 세션이 살아있다는 판정이다. 수신 대기를 이어간다.
                    _lastFrameTime = Time.time;
                }
                yield return new WaitForSeconds(1f);
            }
        }

        private void OnFrameInfoReceived(string json)
        {
            _lastFrameTime = Time.time;
            _frameInfoHandler.Handler(json);
        }

        // ─── 연결 이벤트 핸들러 ──────────────────────────────────────────────

        private void HandleConnected()
        {
            WDebug.Log("[STOMP] 연결됨 – 기존 구독 복구 중");
            _reconnect.ResetRetries();
            _registry.ResubscribeAll(_transport);
        }

        private void HandleDisconnected(string message)
        {
            SystemMessageUI.Instance.ShowMessage(connectionClosed);
            WDebug.Log("[STOMP] 연결 종료: " + message);
        }

        private void HandleError(string error)
        {
            SystemMessageUI.Instance.ShowMessage(connectionDelayed);
            WDebug.LogError("[STOMP] 에러: " + error);
            _reconnect.NotifyConnectionLost();
        }

        private void HandleMaxRetriesExceeded()
        {
            WDebug.LogError("[STOMP] 재연결 불가 – 최대 횟수 초과");
            StartCoroutine(AbandonSession(SessionLossReason.ReconnectAttemptsExhausted, RetryFromScratch));
        }

        /// <summary>세션이 살아있다는 판정을 받았을 때 재연결 예산을 되돌리고 다시 붙는다.</summary>
        private void RetryFromScratch()
        {
            _reconnect.ResetRetries();
            ConnectToServer();
        }

        // ─── 이탈 ───────────────────────────────────────────────────────────

        /// <summary>
        /// 세션 소실을 서버에 신고하고 판정대로 처리한다. 이탈이면 로비로 돌아가고,
        /// 세션이 살아있다는 판정이면 <paramref name="onSessionAlive"/>를 부른 뒤 게임씬에 남는다.
        /// </summary>
        private IEnumerator AbandonSession(SessionLossReason reason, Action onSessionAlive)
        {
            if (_reportingSessionLoss)
            {
                // 다른 이탈 경로가 이미 신고 중이다. 중복 신고하지 않고 그 판정을 함께 따른다.
                // 이탈로 판정되면 로비 로드로 이 코루틴도 함께 사라진다.
                while (_reportingSessionLoss) yield return null;
                onSessionAlive?.Invoke();
                yield break;
            }

            _reportingSessionLoss = true;

            yield return SessionLossReporter.ReportAndLeave(reason, () =>
            {
                _reportingSessionLoss = false;
                onSessionAlive?.Invoke();
            });
        }
    }
}

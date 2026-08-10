using System;
using System.Collections;
using System.Text;
using Data;
using Data.Net;
using LobbyScene;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Global
{
    /// <summary>세션을 포기하게 만든 사유. 진단 로그용이다.</summary>
    public enum SessionLossReason
    {
        /// <summary>게임 서버 STOMP 연결이 제한 시간 안에 열리지 않았다.</summary>
        ConnectTimeout,

        /// <summary>연결은 됐지만 FrameInfo가 제한 시간 동안 오지 않았다.</summary>
        FrameInfoTimeout,

        /// <summary>재연결 시도를 모두 소진했다.</summary>
        ReconnectAttemptsExhausted,

        /// <summary>로비에서 진행 중이라는 세션으로 복구하지 못했다.</summary>
        SessionRecoveryFailed
    }

    /// <summary>
    /// 세션에 더 이상 도달할 수 없을 때 매칭 서버에 신고하고, 서버 판정대로 이탈 여부를 정한다.
    ///
    /// 클라이언트는 세션 종료를 스스로 확정하지 않는다. 신고만 하고, 매칭 서버가 게임 서버에
    /// 확인해서 내린 판정을 따른다. 게임씬의 이탈 경로(연결 타임아웃, FrameInfo 타임아웃,
    /// 재연결 소진)와 로비의 세션 복구 실패가 모두 이 경로로 모인다.
    /// </summary>
    public static class SessionLossReporter
    {
        private const string LobbySceneName = "LobbyScene";
        private const string MessageTable = "SystemMessageUI";

        private static readonly LocalizedString sessionLost = new LocalizedString
            { TableReference = MessageTable, TableEntryReference = "sessionLost" };

        private static readonly LocalizedString sessionLossPending = new LocalizedString
            { TableReference = MessageTable, TableEntryReference = "sessionLossPending" };

        private static readonly LocalizedString sessionStillAlive = new LocalizedString
            { TableReference = MessageTable, TableEntryReference = "sessionStillAlive" };

        /// <summary>
        /// 이탈 사유를 신고하고, 판정이 이탈이면 로비로 돌아간다.
        /// 세션이 살아있다는 판정이면 씬을 그대로 두고 <paramref name="onSessionAlive"/>를 부른다.
        /// 게임씬 호출부용이다.
        /// </summary>
        public static IEnumerator ReportAndLeave(SessionLossReason reason, Action onSessionAlive = null)
        {
            SessionLossVerdict verdict = SessionLossVerdict.NotReported;
            yield return Report(reason, v => verdict = v);

            if (!verdict.LeavesSession())
            {
                ShowMessage(sessionStillAlive);
                onSessionAlive?.Invoke();
                yield break;
            }

            // 소실 확정이면 서버가 이미 티켓을 정리했으므로 로비가 다시 매칭 가능한 상태로 뜬다.
            // 판정 보류면 상태를 단정하지 않고 로비 스냅샷이 서버 상태를 그대로 반영하게 둔다.
            ShowMessage(verdict.ClearsMatchState() ? sessionLost : sessionLossPending);
            SceneManager.LoadScene(LobbySceneName);
        }

        /// <summary>
        /// 이탈 사유를 신고하고 서버 판정만 돌려준다. 씬은 건드리지 않는다.
        /// 이미 로비에 있는 호출부용이다.
        /// </summary>
        public static IEnumerator Report(SessionLossReason reason, Action<SessionLossVerdict> onVerdict)
        {
            string sessionId = SceneContext.MatchInfo?.sessionId;

            if (string.IsNullOrEmpty(sessionId))
            {
                // 신고할 세션 식별자가 없으면 서버 판정을 받을 방법도 없다.
                WDebug.LogWarning($"[SessionLoss] sessionId가 없어 신고를 건너뜁니다. reason={reason}");
                onVerdict?.Invoke(SessionLossVerdict.NotReported);
                yield break;
            }

            WDebug.Log($"[SessionLoss] 세션 소실 신고: reason={reason}, sessionId={sessionId}");

            ServerEndpoint endpoint = ServerList.MatchingServer.Api
                .Path("api", "match", "sessions", sessionId, "report-lost");

            using var request = new UnityWebRequest(endpoint, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);

            yield return request.SendWebRequest();

            // 409, 503 같은 판정도 UnityWebRequest에서는 ProtocolError로 온다.
            // 전송 자체가 실패했을 때만 판정 없음으로 다룬다.
            bool reachedServer = request.result != UnityWebRequest.Result.ConnectionError &&
                                 request.result != UnityWebRequest.Result.DataProcessingError;

            SessionLossVerdict verdict = reachedServer
                ? SessionLossVerdictReader.FromResponseCode(request.responseCode)
                : SessionLossVerdict.NotReported;

            if (reachedServer)
                WDebug.Log($"[SessionLoss] 판정: {verdict} ({request.responseCode})");
            else
                WDebug.LogError($"[SessionLoss] 신고 실패: {request.result} / {request.error}");

            onVerdict?.Invoke(verdict);
        }

        private static void ShowMessage(LocalizedString message)
        {
            // 씬마다 SystemMessageUI가 있으리라 보장할 수 없다. 안내를 못 띄워도 이탈은 막지 않는다.
            if (SystemMessageUI.Instance == null) return;
            SystemMessageUI.Instance.ShowMessage(message);
        }
    }
}

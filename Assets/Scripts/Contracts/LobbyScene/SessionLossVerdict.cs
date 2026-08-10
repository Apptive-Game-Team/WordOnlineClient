namespace LobbyScene
{
    /// <summary>
    /// 세션 소실 신고(<c>POST /api/match/sessions/{sessionId}/report-lost</c>)에 대한 서버 판정.
    ///
    /// 클라이언트는 세션 종료를 스스로 확정하지 않는다. 매칭 서버가 게임 서버에 확인한 뒤 내린
    /// 판정만 따른다.
    /// </summary>
    public enum SessionLossVerdict
    {
        /// <summary>세션 소실이 확정됐다. 로비로 돌아가면 바로 재매칭할 수 있다.</summary>
        Lost,

        /// <summary>세션이 살아있다. 이탈하지 않고 게임씬을 유지하거나 재입장한다.</summary>
        Alive,

        /// <summary>
        /// 게임 서버가 응답하지 않아 판정이 보류됐다. 로비로 돌아가되 매치 상태는 단정하지 않는다.
        /// 서버가 나중에 스스로 정리한다.
        /// </summary>
        Deferred,

        /// <summary>신고 자체가 실패했다. 판정이 없으므로 <see cref="Deferred"/>와 같이 다룬다.</summary>
        NotReported
    }

    /// <summary>세션 소실 신고 응답을 판정으로 옮기는 규칙.</summary>
    public static class SessionLossVerdictReader
    {
        /// <summary>
        /// HTTP 응답 코드를 판정으로 바꾼다.
        ///
        /// <list type="bullet">
        /// <item>2xx: 세션 소실 확정.</item>
        /// <item>404: 이 세션으로 열린 티켓이 없다. 복구할 것이 없으므로 소실과 같이 다룬다.</item>
        /// <item>409: 세션이 살아있다.</item>
        /// <item>503: 게임 서버 무응답으로 판정 보류.</item>
        /// <item>그 외: 판정을 받지 못했다.</item>
        /// </list>
        /// </summary>
        public static SessionLossVerdict FromResponseCode(long responseCode)
        {
            if (responseCode >= 200 && responseCode < 300) return SessionLossVerdict.Lost;

            return responseCode switch
            {
                404 => SessionLossVerdict.Lost,
                409 => SessionLossVerdict.Alive,
                503 => SessionLossVerdict.Deferred,
                _ => SessionLossVerdict.NotReported
            };
        }

        /// <summary>세션을 포기하고 메인 화면으로 돌아가야 하는 판정인지 여부.</summary>
        public static bool LeavesSession(this SessionLossVerdict verdict) =>
            verdict != SessionLossVerdict.Alive;

        /// <summary>매치 상태가 정리됐다고 믿어도 되는 판정인지 여부.</summary>
        public static bool ClearsMatchState(this SessionLossVerdict verdict) =>
            verdict == SessionLossVerdict.Lost;
    }
}

namespace GameScene
{
    /// <summary>연결 대기 중 이번 틱에 할 일.</summary>
    public enum ConnectAttemptDecision
    {
        /// <summary>이번 시도의 예산이 남았다. 계속 기다린다.</summary>
        Wait,

        /// <summary>이번 시도가 예산을 다 썼다. 소켓을 다시 연다.</summary>
        Retry,

        /// <summary>마지막 시도까지 예산을 다 썼다. 연결을 포기한다.</summary>
        GiveUp
    }

    /// <summary>
    /// 게임 서버 STOMP 최초 연결의 재시도 일정.
    ///
    /// 소켓이 끝내 열리지 않는 경우 예전에는 한 번 걸어둔 연결이 10초 예산을 통째로 태우고
    /// 끝났다. 짧은 예산을 여러 번 쓰면 총 대기 시간을 줄이면서도 일시적인 실패를 넘길 수 있다.
    ///
    /// 백그라운드 탭에서 루프가 멈춘 구간은 서버가 늦은 것이 아니므로 예산에서 뺀다.
    /// </summary>
    public sealed class ConnectAttemptSchedule
    {
        public const int DefaultMaxAttempts = 3;

        /// <summary>시도 한 번당 예산. 최대 대기는 이 값 × <see cref="DefaultMaxAttempts"/>다.</summary>
        public const float DefaultAttemptSeconds = 2f;

        private readonly int _maxAttempts;
        private readonly float _attemptSeconds;
        private readonly float _loopStallSeconds;

        private int _attempt = 1;
        private float _elapsed;

        public ConnectAttemptSchedule(
            int maxAttempts = DefaultMaxAttempts,
            float attemptSeconds = DefaultAttemptSeconds,
            float loopStallSeconds = FrameSilenceWatch.DefaultLoopStallSeconds)
        {
            _maxAttempts = maxAttempts;
            _attemptSeconds = attemptSeconds;
            _loopStallSeconds = loopStallSeconds;
        }

        /// <summary>진행 중인 시도 번호. 1부터 센다.</summary>
        public int Attempt => _attempt;

        public int MaxAttempts => _maxAttempts;

        /// <summary>예산을 처음부터 다시 준다.</summary>
        public void Reset()
        {
            _attempt = 1;
            _elapsed = 0f;
        }

        /// <param name="sinceLastTick">직전 틱과의 실시간 간격(초). <c>Time.unscaledDeltaTime</c>.</param>
        public ConnectAttemptDecision Advance(float sinceLastTick)
        {
            // 루프가 멈춰 있던 구간은 서버에 준 시간이 아니다.
            if (sinceLastTick >= _loopStallSeconds) return ConnectAttemptDecision.Wait;

            _elapsed += sinceLastTick;
            if (_elapsed < _attemptSeconds) return ConnectAttemptDecision.Wait;

            _elapsed = 0f;
            if (_attempt >= _maxAttempts) return ConnectAttemptDecision.GiveUp;

            _attempt++;
            return ConnectAttemptDecision.Retry;
        }
    }
}

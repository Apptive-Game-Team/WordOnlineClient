namespace GameScene
{
    /// <summary>연결 요청을 지금 보낼지에 대한 판정.</summary>
    public enum ConnectGateDecision
    {
        /// <summary>새 핸드셰이크를 시작한다.</summary>
        Connect,

        /// <summary>소켓이 이미 열려 있다. 다시 열지 않는다.</summary>
        AlreadyConnected,

        /// <summary>핸드셰이크가 진행 중이다. 끝날 때까지 기다린다.</summary>
        HandshakeInFlight
    }

    /// <summary>
    /// 소켓을 여는 요청이 겹치지 않게 막는 판정기.
    ///
    /// 연결이 열렸다는 사실은 전송 계층의 콜백(WebGL은 JS OnConnected, 네이티브는 CONNECTED
    /// 프레임)이 도착해야 알 수 있다. 그 사이에 재연결 사다리가 다음 칸으로 넘어가면 아직
    /// 끝나지 않은 핸드셰이크 위로 두 번째 핸드셰이크가 겹친다. 겹친 요청은 앞선 소켓을
    /// 닫으므로, 핸드셰이크가 사다리 간격보다 느린 회선에서는 매번 직전 시도를 죽여
    /// 영영 연결되지 않는다. 그래서 진행 중인 핸드셰이크에는 시간을 준다.
    ///
    /// 다만 무한정 기다리면 콜백이 끝내 오지 않는 경우에 재연결이 영구히 막히므로
    /// <see cref="DefaultHandshakeSeconds"/>가 지나면 다시 열 수 있게 풀어준다.
    /// </summary>
    public sealed class StompConnectGate
    {
        /// <summary>
        /// 핸드셰이크 하나에 주는 시간. 최초 연결에서 한 시도에 주는 예산과 같은 값을 쓴다.
        /// 이보다 짧으면 <see cref="ConnectAttemptSchedule"/>의 재시도가 자기가 건 핸드셰이크를
        /// 죽이고, 이보다 길면 응답 없는 핸드셰이크가 재시도를 그만큼 더 막는다.
        /// </summary>
        public const float DefaultHandshakeSeconds = ConnectAttemptSchedule.DefaultAttemptSeconds;

        private readonly float _handshakeSeconds;

        private bool _handshakeInFlight;
        private float _handshakeStartedAt;

        public StompConnectGate(float handshakeSeconds = DefaultHandshakeSeconds)
        {
            _handshakeSeconds = handshakeSeconds;
        }

        /// <summary>아직 결과를 받지 못한 핸드셰이크가 있는지.</summary>
        public bool HandshakeInFlight => _handshakeInFlight;

        /// <param name="transportConnected">전송 계층이 보고하는 현재 연결 상태.</param>
        /// <param name="now">현재 실시간(초). <c>Time.unscaledTime</c>.</param>
        public ConnectGateDecision Decide(bool transportConnected, float now)
        {
            if (transportConnected) return ConnectGateDecision.AlreadyConnected;

            if (_handshakeInFlight && now - _handshakeStartedAt < _handshakeSeconds)
                return ConnectGateDecision.HandshakeInFlight;

            return ConnectGateDecision.Connect;
        }

        /// <summary>소켓을 여는 요청을 실제로 보낸 직후에 부른다.</summary>
        public void NoteHandshakeStarted(float now)
        {
            _handshakeInFlight = true;
            _handshakeStartedAt = now;
        }

        /// <summary>
        /// 핸드셰이크 결과가 나왔을 때 부른다. 성공(Connected), 실패(Errored), 종료(Disconnected)가
        /// 모두 여기에 해당하고, 요청을 보내기 전에 포기한 경우도 마찬가지다.
        /// </summary>
        public void NoteSettled()
        {
            _handshakeInFlight = false;
        }
    }
}

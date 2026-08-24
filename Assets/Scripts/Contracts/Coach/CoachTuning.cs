namespace Coach
{
    /// <summary>
    /// 훈수 표시 간격의 기본값. CoachDirector가 이 값으로 직렬화 필드를 초기화하므로
    /// 인스펙터에서 빌드 없이 조정할 수 있다.
    /// </summary>
    public static class CoachTuning
    {
        /// <summary>힌트와 힌트 사이 최소 간격. 힌트가 꼬리를 물고 뜨는 것을 막는다.</summary>
        public const float GlobalCooldownSeconds = 12f;

        /// <summary>조건이 남아 있어도 이 시간이 지나면 힌트를 내린다. 화면을 계속 가리지 않기 위함이다.</summary>
        public const float MaxVisibleSeconds = 8f;

        /// <summary>씬 진입 직후 이 시간 동안은 어떤 힌트도 띄우지 않는다. 로딩과 연출 구간이다.</summary>
        public const float StartupGraceSeconds = 5f;

        /// <summary>
        /// 힌트가 내려간 뒤에도 이 시간 안에 유저가 해당 행동을 하면 힌트를 따른 것으로 센다.
        /// 읽고 나서 조금 늦게 움직인 유저를 무시한 것으로 처리하지 않기 위함이다.
        /// </summary>
        public const float SatisfyWindowSeconds = 5f;

        /// <summary>무시당한 횟수만큼 다음 노출을 미룬다. 마지막 값이 상한이다.</summary>
        public static readonly float[] IgnoreBackoffSeconds = { 30f, 60f, 120f };
    }
}

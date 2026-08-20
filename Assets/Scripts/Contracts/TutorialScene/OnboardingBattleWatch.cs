namespace TutorialScene
{
    /// <summary>온보딩 마지막 전투에서 씬 전환을 보고 다음에 할 일을 정한다.</summary>
    public enum OnboardingBattleAction
    {
        None,

        /// <summary>아직 전투를 안 했다. 프랙티스 버튼을 누르라고 안내한다.</summary>
        PromptPracticeMatch,

        /// <summary>전투를 마치고 돌아왔다. 마무리 안내 후 온보딩을 끝낸다.</summary>
        CompleteOnboarding,
    }

    /// <summary>
    /// 씬 이름만 보고 판단하는 순수 로직. 유니티 타입에 기대지 않아 그대로 테스트할 수 있다.
    /// </summary>
    public sealed class OnboardingBattleWatch
    {
        private const string LobbySceneName = "LobbyScene";
        private const string GameSceneName = "GameScene";

        // 플레이어가 버튼을 안 눌렀거나 매칭이 실패하면 게임 씬에 못 들어간 채 로비에 남는다.
        // 그 상태로 온보딩을 끝내면 튜토리얼의 마지막 전투를 통째로 건너뛴 셈이 되므로,
        // "전투에 들어갔다"를 따로 기억하고 로비로 돌아온 것만으로는 끝내지 않는다.
        private bool enteredBattle;

        public OnboardingBattleAction OnSceneLoaded(string sceneName)
        {
            if (sceneName == null)
            {
                return OnboardingBattleAction.None;
            }

            if (sceneName.Contains(GameSceneName))
            {
                enteredBattle = true;
                return OnboardingBattleAction.None;
            }

            if (!sceneName.Contains(LobbySceneName))
            {
                return OnboardingBattleAction.None;
            }

            return enteredBattle
                ? OnboardingBattleAction.CompleteOnboarding
                : OnboardingBattleAction.PromptPracticeMatch;
        }
    }
}

using LobbyScene;
using UnityEngine.SceneManagement;

namespace TutorialScene
{
    /// <summary>
    /// 온보딩의 마지막 단계. 목업 전투만 겪은 플레이어를 실제 세션으로 한 번 내보낸다.
    /// </summary>
    /// <remarks>
    /// 그냥 프랙티스 매칭을 시작할 뿐이다. 초보 딱지가 붙은 계정은 서버가 접대 봇으로 배정하므로,
    /// 클라이언트는 상대가 누구인지 알 필요도 없고 알아서도 안 된다. 여기서 상대를 지정하면
    /// 배정 규칙이 서버와 클라이언트 두 곳에 생긴다.
    /// </remarks>
    public class HospitalityBattleState : ITutorialState
    {
        private readonly OnboardingBattleWatch watch = new OnboardingBattleWatch();

        public void Enter()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.HospitalityBattle);
            StartPracticeMatch();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void StartPracticeMatch()
        {
            // 로비 씬이 아직 준비되지 않았으면 아무것도 하지 않는다. 씬이 로드될 때 다시 온다.
            if (LobbySceneViewModel.Instance == null)
            {
                return;
            }

            LobbySceneViewModel.Instance.PlayPracticeMatch();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (watch.OnSceneLoaded(scene.name))
            {
                case OnboardingBattleAction.StartPracticeMatch:
                    StartPracticeMatch();
                    break;
                case OnboardingBattleAction.FinishOnboarding:
                    GlobalTutorialManager.Instance.FinishOnboarding();
                    break;
            }
        }
    }
}

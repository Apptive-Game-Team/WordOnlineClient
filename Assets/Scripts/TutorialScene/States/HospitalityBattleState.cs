using UnityEngine.SceneManagement;

namespace TutorialScene
{
    /// <summary>
    /// 온보딩의 마지막 단계. 목업 전투만 겪은 플레이어를 실제 세션으로 한 번 내보낸다.
    /// </summary>
    /// <remarks>
    /// 매칭을 대신 걸어 주지 않는다. 플레이어가 프랙티스 버튼을 직접 눌러 첫 전투를 시작하고,
    /// 돌아오면 마무리 안내를 받는다. 앞의 로비 스텝들이 버튼을 "설명"만 했다면 여기서는 실제로
    /// 눌러 보게 하는 것이 목적이다.
    ///
    /// 상대는 지정하지 않는다. 초보 딱지가 붙은 계정을 접대 봇으로 배정하는 건 서버 몫이라,
    /// 클라이언트는 상대가 누구인지 알 필요도 없고 알아서도 안 된다.
    /// </remarks>
    public class HospitalityBattleState : ITutorialState
    {
        private readonly OnboardingBattleWatch watch = new OnboardingBattleWatch();

        private LobbyTutorialController lobbyController;

        // 로비 씬이 다시 로드되면 씬 이벤트가 또 온다. 마무리 안내를 두 번 띄우면 두 번째
        // 패널이 이미 끝난 온보딩을 다시 끝내려 든다.
        private bool completing;

        public void Enter()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            GlobalTutorialManager.Instance.SetProgress(OnboardingProgress.HospitalityBattle);
            PromptPracticeMatch();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            HidePrompt();
        }

        // 안내를 띄웠던 컨트롤러를 계속 들고 있어야 Exit에서 패널을 걷을 수 있다. 걷지 않으면
        // 온보딩이 끝난 뒤에도 마지막 안내가 로비에 그대로 남는다.
        private void ShowThrough(LobbyTutorialController controller, System.Action show)
        {
            lobbyController = controller;
            show();
        }

        private void PromptPracticeMatch()
        {
            // 로비 씬이 아직 준비되지 않았으면 아무것도 하지 않는다. 씬이 로드될 때 다시 온다.
            LobbyTutorialController controller = LobbyTutorialController.Instance;
            if (controller == null)
            {
                return;
            }

            ShowThrough(controller, () => controller.ShowPracticeButton(OnPracticeButtonClicked));
        }

        // 버튼을 누르면 로비 뷰모델이 매칭을 걸고 게임 씬으로 넘어간다. 안내는 여기서 걷는다.
        private void OnPracticeButtonClicked()
        {
            HidePrompt();
        }

        private void HidePrompt()
        {
            if (lobbyController == null)
            {
                return;
            }

            lobbyController.HidePracticeButton(OnPracticeButtonClicked);
            lobbyController = null;
        }

        private void CompleteOnboarding()
        {
            if (completing)
            {
                return;
            }

            completing = true;

            LobbyTutorialController controller = LobbyTutorialController.Instance;
            if (controller == null)
            {
                // 마무리 안내를 띄울 곳이 없다면 안내 없이 끝낸다. 안내가 없다고 튜토리얼이
                // 끝나지 않은 채 남는 쪽이 더 나쁘다.
                FinishOnboarding();
                return;
            }

            ShowThrough(controller, () => controller.ShowFirstMatchComplete(FinishOnboarding));
        }

        private static void FinishOnboarding()
        {
            GlobalTutorialManager manager = GlobalTutorialManager.Instance;
            if (manager == null)
            {
                return;
            }

            manager.FinishOnboarding();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (watch.OnSceneLoaded(scene.name))
            {
                case OnboardingBattleAction.PromptPracticeMatch:
                    PromptPracticeMatch();
                    break;
                case OnboardingBattleAction.CompleteOnboarding:
                    CompleteOnboarding();
                    break;
            }
        }
    }
}

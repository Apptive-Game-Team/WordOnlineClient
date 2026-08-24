using NUnit.Framework;
using TutorialScene;

namespace WordOnline.Tests
{
    public class OnboardingBattleWatchTests
    {
        [Test]
        public void CompletesOnboardingWhenTheLobbyComesBackAfterTheBattle()
        {
            var watch = new OnboardingBattleWatch();

            Assert.AreEqual(OnboardingBattleAction.None, watch.OnSceneLoaded("GameScene"));
            Assert.AreEqual(OnboardingBattleAction.CompleteOnboarding, watch.OnSceneLoaded("LobbyScene"));
        }

        // 매칭 실패로 로비에 남은 경우다. 여기서 온보딩을 끝내면 마지막 전투를 통째로 건너뛴다.
        [Test]
        public void PromptsAgainWhenTheLobbyLoadsWithoutABattleHavingStarted()
        {
            var watch = new OnboardingBattleWatch();

            Assert.AreEqual(OnboardingBattleAction.PromptPracticeMatch, watch.OnSceneLoaded("LobbyScene"));
        }

        [Test]
        public void IgnoresUnrelatedScenes()
        {
            var watch = new OnboardingBattleWatch();

            Assert.AreEqual(OnboardingBattleAction.None, watch.OnSceneLoaded("MagicBookScene"));
            Assert.AreEqual(OnboardingBattleAction.None, watch.OnSceneLoaded(null));
        }

        [Test]
        public void StaysFinishedOnceTheBattleHasBeenPlayed()
        {
            var watch = new OnboardingBattleWatch();
            watch.OnSceneLoaded("GameScene");
            watch.OnSceneLoaded("LobbyScene");

            Assert.AreEqual(OnboardingBattleAction.CompleteOnboarding, watch.OnSceneLoaded("LobbyScene"));
        }
    }
}

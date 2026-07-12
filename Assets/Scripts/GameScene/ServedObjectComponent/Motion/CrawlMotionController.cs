namespace GameScene.ServedObjectComponent.Motion
{
    public class CrawlMotionController : DOTweenMotionController
    {
        [UnityEngine.SerializeField] private MotionSoundProfile soundProfile = MotionSoundProfile.Squish;

        protected override void Awake()
        {
            DOTweenAction.CrawlMob(appliedTransform);
            StartMotionSound(soundProfile, 0.25f, 0.5f);
        }
    }
}

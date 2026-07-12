namespace GameScene.ServedObjectComponent.Motion
{
    public class StumbleMotionController : DOTweenMotionController
    {
        [UnityEngine.SerializeField] private MotionSoundProfile soundProfile = MotionSoundProfile.Stumble;

        protected override void Awake()
        {
            DOTweenAction.SwingStormIdle(appliedTransform);
            StartMotionSound(soundProfile, 0.5f, 1f);
        }
    }
}

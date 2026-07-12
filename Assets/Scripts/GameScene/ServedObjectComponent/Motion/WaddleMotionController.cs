namespace GameScene.ServedObjectComponent.Motion
{
    public class WaddleMotionController : DOTweenMotionController
    {
        [UnityEngine.SerializeField] private MotionSoundProfile soundProfile = MotionSoundProfile.Heavy;

        protected override void Awake()
        {
            DOTweenAction.WaddleBigMob(appliedTransform);
            StartMotionSound(soundProfile, 1.5f, 1.5f);
        }
    }
}

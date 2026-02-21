namespace Scripts.GameScene.ServedObjectComponent.Motion
{
    public class WaddleMotionController : DOTweenMotionController
    {
        protected override void Awake()
        {
              DOTweenAction.WaddleBigMob(appliedTransform);
        }
    }
}
namespace Script.GameScene
{
    public class HoverMotionController : DOTweenMotionController
    {
        protected override void Awake()
        {
           DOTweenAction.HoverMob(appliedTransform);
        }
    }
}
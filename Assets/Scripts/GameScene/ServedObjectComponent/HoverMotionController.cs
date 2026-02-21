using Scripts.GameScene.ServedObjectComponent.Motion;

namespace Scripts.GameScene.ServedObjectComponent
{
    public class HoverMotionController : DOTweenMotionController
    {
        protected override void Awake()
        {
           DOTweenAction.HoverMob(appliedTransform);
        }
    }
}
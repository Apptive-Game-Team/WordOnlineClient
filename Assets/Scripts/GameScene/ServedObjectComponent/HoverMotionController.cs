using GameScene.ServedObjectComponent.Motion;

namespace GameScene.ServedObjectComponent
{
    public class HoverMotionController : DOTweenMotionController
    {
        protected override void Awake()
        {
           DOTweenAction.HoverMob(appliedTransform);
        }
    }
}
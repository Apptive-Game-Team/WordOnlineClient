namespace Scripts.GameScene.ServedObjectComponent.Motion
{
    public class StumbleMotionController : DOTweenMotionController
    {
        protected override void Awake()
        {
           DOTweenAction.SwingStormIdle(appliedTransform);  
        }
    }
}
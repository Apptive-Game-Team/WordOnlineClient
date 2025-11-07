namespace Script.GameScene
{
    public class StumbleMotionController : DOTweenMotionController
    {
        protected override void Awake()
        {
           DOTweenAction.SwingStormIdle(appliedTransform);  
        }
    }
}
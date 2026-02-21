namespace GameScene.ServedObjectComponent.Motion
{
    public class CrawlMotionController : DOTweenMotionController
    {
        protected override void Awake()
        {
           DOTweenAction.CrawlMob(appliedTransform);  
        }
    }
}
namespace Script.GameScene
{
    public class CrawlMotionController : DOTweenMotionController
    {
        protected override void Awake()
        {
           DOTweenAction.CrawlMob(appliedTransform);  
        }
    }
}
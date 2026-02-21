namespace GameScene
{
    public class PopSelfDestroyer : PopSelfController
    {
        protected override void Start()
        {
            base.Start();
            Destroy(gameObject, duration);
        }
    }
}

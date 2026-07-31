namespace GameScene.ServedObjectComponent.Effect
{
    /// <summary>
    /// Shows an emergence frame right after the object spawns, then settles into the idle frame.
    /// Only fires on a real spawn — objects that appear through a full-state sync skip it, because
    /// <see cref="ServedObject.OnSpawned"/> is only raised when the spawn presentation should play.
    /// </summary>
    public sealed class SpawnSpriteSwapController : TimedSpriteSwapController
    {
        protected override void Subscribe(ServedObject owner)
        {
            owner.OnSpawned += Play;
        }

        protected override void Unsubscribe(ServedObject owner)
        {
            owner.OnSpawned -= Play;
        }
    }
}

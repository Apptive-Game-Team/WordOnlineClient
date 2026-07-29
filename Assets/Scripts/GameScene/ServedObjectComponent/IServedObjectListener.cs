namespace GameScene.ServedObjectComponent
{
    /// <summary>
    /// Implemented by prefab-authored components that need the <see cref="ServedObject"/> of the
    /// object they live on.
    /// <para>
    /// <c>ObjectSpawner</c> binds every listener under a spawned object explicitly, right after the
    /// ServedObject is configured, so a listener never has to guess whether the ServedObject exists
    /// yet. Use <see cref="ServedObjectBehaviour"/> instead of implementing this directly unless the
    /// component needs its own MonoBehaviour lifetime handling.
    /// </para>
    /// </summary>
    public interface IServedObjectListener
    {
        void Bind(ServedObject servedObject);
    }
}

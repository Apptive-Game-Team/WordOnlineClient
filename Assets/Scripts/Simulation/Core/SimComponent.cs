namespace Simulation.Core
{
    /// <summary>
    /// Abstract base component attached to a SimGameObject.
    /// Mirrors the server's Component pattern with lifecycle methods.
    /// </summary>
    public abstract class SimComponent
    {
        public SimGameObject GameObject { get; internal set; }

        public SimWorld World => GameObject.World;

        public abstract void Start();
        public abstract void Update();
        public abstract void OnDestroy();
    }
}

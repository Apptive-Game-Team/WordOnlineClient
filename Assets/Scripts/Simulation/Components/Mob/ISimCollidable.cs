namespace Simulation.Core
{
    public interface ISimCollidable
    {
        void OnCollision(SimGameObject other);
    }
}

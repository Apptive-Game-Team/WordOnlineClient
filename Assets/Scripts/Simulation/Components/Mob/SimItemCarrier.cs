namespace Simulation.Core
{
    /// <summary>
    /// Picks up and carries totem items on collision.
    /// </summary>
    public class SimItemCarrier : SimComponent, ISimCollidable
    {
        private SimTotem _item;

        public override void Start() { }
        public override void OnDestroy() { }

        public override void Update()
        {
            if (_item != null)
                _item.GameObject.SetPosition(GameObject.Position);
        }

        public void OnCollision(SimGameObject other)
        {
            if (_item != null) return;
            var totem = other.GetComponent<SimTotem>();
            if (totem != null)
                _item = totem;
        }
    }
}

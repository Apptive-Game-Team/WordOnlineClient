namespace Simulation.Core
{
    public class SimCircleCollider : SimCollider
    {
        public Fix64 Radius { get; }

        public SimCircleCollider(Fix64 radius, bool isTrigger) : base(isTrigger)
        {
            Radius = radius;
        }

        public override bool IsCollidingWith(SimCollider other)
        {
            if (other is SimCircleCollider circle)
            {
                Fix64 dist = Position.Distance(circle.Position);
                return dist < Radius + circle.Radius;
            }
            if (other is SimEdgeCollider edge)
                return edge.IsCollidingWith(this);
            return false;
        }

        public override SimVector3 GetDisplacement(SimCollider other)
        {
            if (other is SimCircleCollider circle)
                return Position - circle.Position;
            if (other is SimEdgeCollider edge)
                return edge.GetDisplacement(this) * -1;
            return SimVector3.Zero;
        }
    }
}

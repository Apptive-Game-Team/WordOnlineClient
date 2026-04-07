namespace Simulation.Core
{
    public class SimEdgeCollider : SimCollider
    {
        private readonly SimVector3 _relPoint1;
        private readonly SimVector3 _relPoint2;

        public SimVector3 Point1 => _relPoint1 + Position.ToVector2().ToVector3();
        public SimVector3 Point2 => _relPoint2 + Position.ToVector2().ToVector3();

        public SimEdgeCollider(SimVector3 point1, SimVector3 point2, bool isTrigger) : base(isTrigger)
        {
            _relPoint1 = point1;
            _relPoint2 = point2;
        }

        public override bool IsCollidingWith(SimCollider other)
        {
            if (other is SimEdgeCollider edge)
                return SegmentsIntersect(Point1, Point2, edge.Point1, edge.Point2);
            if (other is SimCircleCollider circle)
                return IsCircleColliding(circle);
            return false;
        }

        public override SimVector3 GetDisplacement(SimCollider other)
        {
            if (other is SimCircleCollider circle)
            {
                SimVector3 proj = GetProjection(circle.Position, Point1, Point2);
                return proj - other.Position;
            }
            return SimVector3.Zero;
        }

        private bool IsCircleColliding(SimCircleCollider circle)
        {
            Fix64 dist = DistancePointToSegment(circle.Position, Point1, Point2);
            return dist <= circle.Radius;
        }

        private static SimVector3 GetProjection(SimVector3 center, SimVector3 p1, SimVector3 p2)
        {
            SimVector3 ab = p2 - p1;
            SimVector3 ap = center - p1;
            Fix64 abLenSq = ab.Dot(ab);
            if (abLenSq == Fix64.Zero) return p1;
            Fix64 t = ap.Dot(ab) / abLenSq;
            t = SimMath.Clamp(t, Fix64.Zero, Fix64.One);
            return p1 + ab * t;
        }

        private static Fix64 DistancePointToSegment(SimVector3 center, SimVector3 p1, SimVector3 p2)
        {
            SimVector3 proj = GetProjection(center, p1, p2);
            return center.Distance(proj);
        }

        private static bool SegmentsIntersect(SimVector3 p1, SimVector3 p2, SimVector3 q1, SimVector3 q2)
        {
            return Ccw(p1, q1, q2) != Ccw(p2, q1, q2) &&
                   Ccw(p1, p2, q1) != Ccw(p1, p2, q2);
        }

        private static bool Ccw(SimVector3 a, SimVector3 b, SimVector3 c)
        {
            return (c.Y - a.Y) * (b.X - a.X) > (b.Y - a.Y) * (c.X - a.X);
        }
    }
}

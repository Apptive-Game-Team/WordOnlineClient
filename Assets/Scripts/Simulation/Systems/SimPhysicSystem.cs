using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Per-frame physics system: collision detection, response, then apply velocities.
    /// Mirrors the server's PhysicSystem.
    /// </summary>
    public class SimPhysicSystem
    {
        private readonly List<(SimGameObject a, SimGameObject b)> _collidedPairs = new();

        public void Update(SimWorld world)
        {
            var active = world.GetActiveGameObjects();
            CalculateCollisions(active);
            ApplyCollisionResponses();
            CheckAndHandleCollisions(active);
            OnUpdateEnd(active);
        }

        private void CalculateCollisions(List<SimGameObject> objects)
        {
            _collidedPairs.Clear();
            for (int i = 0; i < objects.Count; i++)
            {
                var a = objects[i];
                if (a.IsDestroyed || !a.HasComponent<SimCollider>()) continue;

                for (int j = i + 1; j < objects.Count; j++)
                {
                    var b = objects[j];
                    if (b.IsDestroyed || !b.HasComponent<SimCollider>()) continue;

                    if (SimPhysics.IsColliding(a, b))
                        _collidedPairs.Add((a, b));
                }
            }
        }

        private void ApplyCollisionResponses()
        {
            foreach (var (a, b) in _collidedPairs)
            {
                var rbA = a.GetComponent<SimRigidBody>();
                var rbB = b.GetComponent<SimRigidBody>();

                var collidersA = a.GetComponents<SimCollider>();
                var collidersB = b.GetComponents<SimCollider>();

                foreach (var ca in collidersA)
                {
                    if (ca.IsTrigger) continue;
                    foreach (var cb in collidersB)
                    {
                        if (cb.IsTrigger) continue;

                        Fix64 invMassA = rbA != null ? rbA.InvMass : Fix64.Zero;
                        Fix64 invMassB = rbB != null ? rbB.InvMass : Fix64.Zero;

                        SimVector3 disp = ca.GetDisplacement(cb);
                        Fix64 dispMag = disp.SqrMagnitude;
                        if (dispMag < Fix64.Epsilon)
                            continue;

                        SimVector3 normal = disp.Normalized;

                        SimVector3 velA = rbA != null ? rbA.Velocity : SimVector3.Zero;
                        SimVector3 velB = rbB != null ? rbB.Velocity : SimVector3.Zero;
                        SimVector3 relVel = velA - velB;
                        Fix64 sepVel = relVel.Dot(normal);

                        if (sepVel > Fix64.Zero) continue; // already separating

                        Fix64 totalInvMass = invMassA + invMassB;
                        if (totalInvMass == Fix64.Zero) continue;

                        Fix64 restitution = Fix64.One;
                        Fix64 impulseMag = -(Fix64.One + restitution) * sepVel / totalInvMass;
                        impulseMag = SimMath.Clamp(impulseMag, Fix64.MinusOne, Fix64.One);

                        SimVector3 impulse = normal * impulseMag;
                        if (invMassA > Fix64.Zero && rbA != null)
                            rbA.AddVelocity(impulse * invMassA);
                        if (invMassB > Fix64.Zero && rbB != null)
                            rbB.AddVelocity((impulse * invMassB) * -1);
                    }
                }
            }
        }

        private void CheckAndHandleCollisions(List<SimGameObject> objects)
        {
            foreach (var (a, b) in _collidedPairs)
            {
                if (a.Master == b.Master && b.Master != Master.None) continue;
                if (a.IsDestroyed || b.IsDestroyed) continue;

                foreach (var c in a.GetComponents<ISimCollidable>())
                    c.OnCollision(b);
                foreach (var c in b.GetComponents<ISimCollidable>())
                    c.OnCollision(a);
            }
        }

        private void OnUpdateEnd(List<SimGameObject> objects)
        {
            foreach (var obj in objects)
            {
                var rb = obj.GetComponent<SimRigidBody>();
                rb?.ApplyVelocity();

                var zp = obj.GetComponent<SimZPhysics>();
                if (zp != null)
                {
                    if (zp.CanHover()) zp.ApplyHover();
                    else zp.ApplyZForce();
                }
            }
        }
    }
}

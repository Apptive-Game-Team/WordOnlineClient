using GameScene.Dto.Projectile;
using GameScene.ServedObjectComponent;
using UnityEngine;

namespace GameScene.Object.Projectile
{
    public class ProjectileUtil
    {
        public static Quaternion GetRotation(ProjectileDto dto)
        {
            Vector3 start = GetPosition(dto.start);
            Vector3 end = GetPosition(dto.end);
            Camera camera = Camera.main;
            Vector3 dir = camera != null
                ? camera.WorldToScreenPoint(end) - camera.WorldToScreenPoint(start)
                : end - start;
            
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion screenRotation = Quaternion.AngleAxis(angle, Vector3.forward);

            return camera != null
                ? camera.transform.rotation * screenRotation
                : screenRotation;
        }

        public static Vector3 GetPosition(ProjectileTarget target)
        {
            switch (target)
            {
                case PositionProjectileTarget position:
                    return position.ToVector3();
                case ReferenceProjectileTarget reference:
                    ServedObject servedObject = ObjectContainer.Instance.FindById(reference.id);
                    return servedObject != null ? servedObject.transform.position : Vector3.zero;
                default:
                    return Vector3.zero;
            }
        }
    }
}

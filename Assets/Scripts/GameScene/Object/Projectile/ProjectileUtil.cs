using Scripts.GameScene.Dto.Projectile;
using Scripts.GameScene.ServedObjectComponent;
using UnityEngine;

namespace Scripts.GameScene.Object.Projectile
{
    public class ProjectileUtil
    {
        public static Quaternion GetRotation(ProjectileDto dto)
        {
            Vector3 start = ZVisualizer.CalculateZAppliedPosition(GetPosition(dto.start));
            Vector3 end = ZVisualizer.CalculateZAppliedPosition(GetPosition(dto.end));
            Vector3 dir = end - start;
            
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            
            return Quaternion.Euler(0, 0, angle);
        }

        public static Vector3 GetPosition(ProjectileTarget target)
        {
            switch (target.targetType)
            {
                case "position":
                    return new Vector3(target.x, target.y, target.z);
                case "reference":
                    ServedObject servedObject = ObjectContainer.Instance.FindById(target.id);
                    if (servedObject != null)
                    {
                        return servedObject.transform.position;
                    }
                    return Vector3.zero;
                default:
                    return Vector3.zero;
            }
        }
    }
}
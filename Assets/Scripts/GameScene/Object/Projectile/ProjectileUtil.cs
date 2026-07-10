using GameScene.Dto.Projectile;
using GameScene.PopupBook;
using GameScene.ServedObjectComponent;
using UnityEngine;

namespace GameScene.Object.Projectile
{
    public class ProjectileUtil
    {
        public static Quaternion GetRotation(ProjectileDto dto)
        {
            Vector3 start = ZVisualizer.CalculateZAppliedPosition(GetPosition(dto.start));
            Vector3 end = ZVisualizer.CalculateZAppliedPosition(GetPosition(dto.end));
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
            switch (target.targetType)
            {
                case "position":
                    return GameCoordinateConverter.ToUnity(new Vector3(target.x, target.y, target.z));
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

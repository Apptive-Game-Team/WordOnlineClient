using GameScene.Dto.Projectile;
using GameScene.ServedObjectComponent;
using UnityEngine;

namespace GameScene.Object.Projectile
{
    public class ProjectileUtil
    {
        public static Quaternion GetRotation(ProjectileDto dto)
        {
            return GetRotation(GetPosition(dto.start), GetPosition(dto.end));
        }

        public static Quaternion GetRotation(Vector3 start, Vector3 end)
        {
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

        /// <summary>
        /// Distance from start to end measured inside the plane GetRotation orients the sprite in.
        /// The world is a tilted 2.5D view under a perspective camera, so the world distance
        /// between two points is not the distance the sprite has to cover on screen: a delta along
        /// world +Z is foreshortened while a delta along world +X is not. Rounding the end point
        /// through screen space at the start point's depth gives the one world point that both
        /// lies in the sprite's plane and lands on the target's pixel.
        /// </summary>
        public static float GetCameraPlaneLength(Vector3 start, Vector3 end)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return Vector3.Distance(start, end);
            }

            Vector3 startScreen = camera.WorldToScreenPoint(start);
            Vector3 endScreen = camera.WorldToScreenPoint(end);
            Vector3 reachable = camera.ScreenToWorldPoint(new Vector3(endScreen.x, endScreen.y, startScreen.z));

            return Vector3.Distance(start, reachable);
        }

        /// <summary>
        /// Screen-up expressed in world space. Sprites are billboarded to the tilted 2.5D camera,
        /// so a height read off a sprite is a distance along this, not along world up: world up is
        /// foreshortened by the tilt and carries the point away in depth as well.
        /// Mirrors ServedObject.GetAnchorUpDirection, which places speech bubbles the same way.
        /// </summary>
        public static Vector3 GetScreenUp()
        {
            Camera camera = Camera.main;
            return camera != null ? camera.transform.up : Vector3.up;
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

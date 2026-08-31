using GameScene.Dto.Projectile;
using UnityEngine;

namespace GameScene.Object.Projectile
{
    /// <summary>
    /// Stretches a side-on effect sprite between the two positions supplied by the server.
    /// The span is measured in screen space so it remains endpoint-accurate with the
    /// isometric game camera, including ground-to-air shots.
    /// </summary>
    public sealed class LineProjectile : MonoBehaviour, IProjectile
    {
        [SerializeField] private Transform actualObject;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Init(ProjectileDto projectileDto)
        {
            Vector3 start = ProjectileUtil.GetPosition(projectileDto.start);
            Vector3 end = ProjectileUtil.GetPosition(projectileDto.end);
            transform.position = Vector3.Lerp(start, end, 0.5f);
            transform.rotation = ProjectileUtil.GetRotation(projectileDto);

            float width = spriteRenderer.sprite != null
                ? spriteRenderer.sprite.bounds.size.x
                : 1f;
            Vector3 scale = actualObject.localScale;
            scale.x = GetVisibleWorldLength(start, end) / Mathf.Max(width, Mathf.Epsilon);
            actualObject.localScale = scale;
        }

        private static float GetVisibleWorldLength(Vector3 start, Vector3 end)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return Vector3.Distance(start, end);
            }

            Vector3 screenStart = camera.WorldToScreenPoint(start);
            Vector3 screenEnd = camera.WorldToScreenPoint(end);
            float depth = camera.WorldToScreenPoint(Vector3.Lerp(start, end, 0.5f)).z;
            Vector3 origin = camera.ScreenToWorldPoint(new Vector3(0f, 0f, depth));
            Vector3 screenSpan = camera.ScreenToWorldPoint(new Vector3(
                Vector2.Distance(screenStart, screenEnd),
                0f,
                depth));
            return Vector3.Distance(origin, screenSpan);
        }
    }
}

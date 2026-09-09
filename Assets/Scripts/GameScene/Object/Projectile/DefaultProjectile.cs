using DG.Tweening;
using GameScene.Dto.Projectile;
using GameScene.ServedObjectComponent;
using UnityEngine;

namespace GameScene.Object.Projectile
{
    public class DefaultProjectile : MonoBehaviour, IProjectile
    {

        [SerializeField] private Transform actualObject;
        [SerializeField] private float arcHeight = 0f;

        public void Init(ProjectileDto projectileDto)
        {
            actualObject.rotation = ProjectileUtil.GetRotation(projectileDto);
            transform.position = ProjectileUtil.GetPosition(projectileDto.start);

            // The root, not actualObject, carries the horizontal move, so Shadow (parented to the root)
            // keeps tracking the landing point on the ground while actualObject rises above it.
            switch (projectileDto.end)
            {
                case PositionProjectileTarget position:
                    transform.DOMove(position.ToVector3(), projectileDto.duration)
                        .SetEase(Ease.Linear);
                    break;
                case ReferenceProjectileTarget reference:
                    ServedObject targetObject = ObjectContainer.Instance.FindById(reference.id);
                    if (targetObject == null)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    MoveTo(targetObject.transform, projectileDto.duration);
                    break;
            }

            if (arcHeight > 0f && actualObject != null)
            {
                AnimateArc(projectileDto.duration);
            }
        }

        // Same parabola the server's CraterEmber uses: height = 4 * arcHeight * progress * (1 - progress).
        private void AnimateArc(float duration)
        {
            float baseLocalY = actualObject.localPosition.y;

            DOTween.To(() => 0f, progress =>
                {
                    Vector3 localPosition = actualObject.localPosition;
                    localPosition.y = baseLocalY + 4f * arcHeight * progress * (1f - progress);
                    actualObject.localPosition = localPosition;
                }, 1f, duration)
                .SetEase(Ease.Linear)
                .SetLink(gameObject);
        }

        private void MoveTo(Transform target, float duration)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = target != null ? target.position : startPos;

            DOTween.To(() => 0f, v =>
                {
                    if (target)
                    {
                        endPos = target.position;
                    }
                    transform.position = Vector3.Lerp(startPos, endPos, v);
                }, 1f, duration)
                .SetEase(Ease.Linear);
        }
    }
}

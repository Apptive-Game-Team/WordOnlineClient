using DG.Tweening;
using Script.GameScene.Dto.Projectile;
using UnityEngine;

namespace Script.GameScene.Object.Projectile
{
    public class DefaultProjectile : MonoBehaviour, IProjectile
    {

        [SerializeField] private Transform actualObject;
        
        public void Init(ProjectileDto projectileDto)
        {
            actualObject.rotation = ProjectileUtil.GetRotation(projectileDto);
            transform.position = ProjectileUtil.GetPosition(projectileDto.start);
            
            switch (projectileDto.end.targetType)
            {
                case "position":
                    transform.DOMove(projectileDto.end.GetVector3(), projectileDto.duration)
                        .SetEase(Ease.Linear);
                    break;
                case "reference":
                    ServedObject targetObject = ObjectContainer.Instance.FindById(projectileDto.end.id);
                    if (targetObject == null)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    MoveTo(targetObject.transform, projectileDto.duration);
                    break;
            }
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
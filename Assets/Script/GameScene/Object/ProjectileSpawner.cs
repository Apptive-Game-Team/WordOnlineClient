using DG.Tweening;
using Script.GameScene.Dto.Projectile;
using UnityEngine;

namespace Script.GameScene.Object
{
    public class ProjectileSpawner : MonoBehaviour
    {
        public static ProjectileSpawner Instance;
        
        private void Awake()
        {
            Instance = this;
        }
        
        [SerializeField] private ObjectContainer objectContainer;
        
        public void Spawn(ProjectileDto dto)
        {
            GameObject prefabs = GetPrefab(dto.type);
            if (prefabs == null) return;
            
            GameObject projectileObject = Instantiate(prefabs, GetPosition(dto.start), GetRotation(dto));

            switch (dto.end.targetType)
            {
                case "position":
                    projectileObject.transform.DOMove(dto.end.GetVector3(), 0.2f);
                    MoveTo(projectileObject, null, dto.duration);
                    break;
                case "object":
                    ServedObject targetObject = objectContainer.FindById(dto.end.id);
                    MoveTo(projectileObject, targetObject.transform, dto.duration);
                    break;
            }
        }

        private Quaternion GetRotation(ProjectileDto dto)
        {
            Vector3 start = GetPosition(dto.start);
            Vector3 end = GetPosition(dto.end);
            Vector3 dir = end - start;
            
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            
            return Quaternion.Euler(0, 0, angle + 180f);
        }

        private Vector3 GetPosition(ProjectileTarget target)
        {
            switch (target.targetType)
            {
                case "position":
                    return new Vector3(target.x, target.y, target.z);
                case "object":
                    ServedObject servedObject = objectContainer.FindById(target.id);
                    if (servedObject != null)
                    {
                        return servedObject.transform.position;
                    }

                    return Vector3.zero;
                default:
                    return Vector3.zero;
            }
        }

        private void MoveTo(GameObject gameObject, Transform target, float duration)
        {
            Vector3 startPos = gameObject.transform.position;
            DOTween.To(() => 0f, v =>
                {
                    transform.position = Vector3.Lerp(startPos, target.position, v);
                }, 1f, duration)
                .SetEase(Ease.Linear);
        }
        
        private GameObject GetPrefab(string type)
        {
            GameObject prefab = Resources.Load<GameObject>($"Projectiles/{type}");
            if (prefab == null)
            {
                Debug.LogError($"Projectile prefab not found: {type}");
            }
            return prefab;
        }
    }
}
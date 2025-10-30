using DG.Tweening;
using Script.GameScene.Dto.Projectile;
using Script.Global;
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
        
        // private void Start()
        // {
        //     Spawn(new ProjectileDto()
        //     {
        //         type = "ElectricShot",
        //         duration = 5.0f,
        //         start = new ProjectileTarget()
        //         {
        //             targetType = "position",
        //             x = 0,
        //             y = 0,
        //             z = 0
        //         },
        //         end = new ProjectileTarget()
        //         {
        //             targetType = "position",
        //             x = 10,
        //             y = 10,
        //             z = 10
        //         }
        //     });
        // }
        
        [SerializeField] private ObjectContainer objectContainer;
        
        public void Spawn(ProjectileDto dto)
        {
            WDebug.Log("ProjectileSpawner Spawn called for type: " + dto.type);
            GameObject prefabs = GetPrefab(dto.type);
            if (prefabs == null) return;
            
            GameObject projectileObject = Instantiate(prefabs, GetPosition(dto.start), GetRotation(dto));

            switch (dto.end.targetType)
            {
                case "position":
                    projectileObject.transform.DOMove(dto.end.GetVector3(), dto.duration)
                        .SetEase(Ease.Linear);
                    break;
                case "reference":
                    ServedObject targetObject = objectContainer.FindById(dto.end.id);
                    MoveTo(projectileObject, targetObject.transform, dto.duration);
                    break;
            }
            
            Destroy(projectileObject, dto.duration);
        }

        private Quaternion GetRotation(ProjectileDto dto)
        {
            Vector3 start = GetPosition(dto.start);
            Vector3 end = GetPosition(dto.end);
            Vector3 dir = end - start;
            
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            
            return Quaternion.Euler(0, 0, angle);
        }

        private Vector3 GetPosition(ProjectileTarget target)
        {
            switch (target.targetType)
            {
                case "position":
                    return new Vector3(target.x, target.y, target.z);
                case "reference":
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
                    gameObject.transform.position = Vector3.Lerp(startPos, target.position, v);
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
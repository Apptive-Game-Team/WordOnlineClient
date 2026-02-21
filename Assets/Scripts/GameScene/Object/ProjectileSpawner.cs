using GameScene.Dto.Projectile;
using GameScene.Object.Projectile;
using Global;
using UnityEngine;

namespace GameScene.Object
{
    public class ProjectileSpawner : LocalSingletonObject<ProjectileSpawner>
    {
        
        public void Spawn(ProjectileDto dto)
        {
            WDebug.Log("ProjectileSpawner Spawn called for type: " + dto.type);
            GameObject prefabs = GetPrefab(dto.type);
            
            if (prefabs == null) return;
            
            GameObject projectileObject = Instantiate(prefabs);

            IProjectile projectile = projectileObject.GetComponent<IProjectile>();
            
            Destroy(projectileObject, dto.duration);
            
            projectile.Init(dto);
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
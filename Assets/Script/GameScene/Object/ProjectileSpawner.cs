using DG.Tweening;
using Script.GameScene.Dto.Projectile;
using Script.GameScene.Object.Projectile;
using Script.Global;
using UnityEngine;

namespace Script.GameScene.Object
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
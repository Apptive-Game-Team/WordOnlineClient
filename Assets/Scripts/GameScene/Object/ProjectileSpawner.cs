using GameScene.Dto;
using GameScene.Dto.Projectile;
using GameScene.Object.Projectile;
using GameScene.ServedObjectComponent;
using Global;
using UnityEngine;

namespace GameScene.Object
{
    public class ProjectileSpawner : LocalSingletonObject<ProjectileSpawner>
    {
        
        public void Spawn(ProjectileDto dto)
        {
            WDebug.Log("ProjectileSpawner Spawn called for type: " + dto.type);

            if (ShouldSuppressStormStagImpactProjectile(dto))
            {
                WDebug.Log("Suppressed ElectricShot visual for Storm Stag charge impact.");
                return;
            }

            GameObject prefabs = GetPrefab(dto.type);
            
            if (prefabs == null) return;
            
            GameObject projectileObject = Instantiate(prefabs);

            IProjectile projectile = projectileObject.GetComponent<IProjectile>();
            
            Destroy(projectileObject, dto.duration);
            
            projectile.Init(dto);
        }

        private static bool ShouldSuppressStormStagImpactProjectile(ProjectileDto dto)
        {
            if (!(dto.start is ReferenceProjectileTarget sourceReference))
            {
                return false;
            }

            ServedObject source = ObjectContainer.Instance.FindById(sourceReference.id);
            return source != null
                && StormStagChargeImpactRules.ShouldSuppressProjectile(dto.type, source.ActiveEffects);
        }
        
        
        private GameObject GetPrefab(string type)
        {
            string resourceType = type == "ElectricAbsorb" ? "ElectricShot" : type;
            GameObject prefab = Resources.Load<GameObject>($"Projectiles/{resourceType}");
            if (prefab == null)
            {
                Debug.LogError($"Projectile prefab not found: {type}");
            }
            return prefab;
        }
    }
}

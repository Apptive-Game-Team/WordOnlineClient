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
        private const string ShockOverloadSecondaryType = "ShockOverloadSecondary";
        private const string ShockOverloadPrefabPath = "Prefabs/ShockOverload";
        private const float ShockOverloadSecondaryScale = 0.6f;

        public void Spawn(ProjectileDto dto)
        {
            WDebug.Log("ProjectileSpawner Spawn called for type: " + dto.type);

            if (dto.type == "BoulderStrikeImpact")
            {
                SpawnBoulderStrikeImpact(dto);
                return;
            }

            if (dto.type == "SpiritBombBeam")
            {
                SpawnSpiritBombBeam(dto);
                return;
            }

            if (ShouldSuppressStormStagImpactProjectile(dto))
            {
                WDebug.Log("Suppressed ElectricShot visual for Storm Stag charge impact.");
                return;
            }

            if (TrySpawnShockOverloadSecondary(dto))
            {
                return;
            }

            GameObject prefabs = GetPrefab(dto.type);
            
            if (prefabs == null) return;
            
            GameObject projectileObject = Instantiate(prefabs);

            IProjectile projectile = projectileObject.GetComponent<IProjectile>();
            
            Destroy(projectileObject, dto.duration);
            
            projectile.Init(dto);
        }

        private static void SpawnBoulderStrikeImpact(ProjectileDto dto)
        {
            GameObject impactPrefab = Resources.Load<GameObject>("Prefabs/RockExplode");
            if (impactPrefab == null)
            {
                Debug.LogError("RockExplode prefab not found for BoulderStrikeImpact.");
                return;
            }

            GameObject impact = Instantiate(
                impactPrefab,
                ProjectileUtil.GetPosition(dto.start),
                impactPrefab.transform.rotation);
            impact.transform.localScale *= 0.65f;
            Destroy(impact, dto.duration);
        }

        private static void SpawnSpiritBombBeam(ProjectileDto dto)
        {
            GameObject projectileObject = new GameObject("SpiritBombBeam");
            SpiritBombBeamProjectile projectile = projectileObject.AddComponent<SpiritBombBeamProjectile>();
            Destroy(projectileObject, dto.duration);
            projectile.Init(dto);
        }

        private bool TrySpawnShockOverloadSecondary(ProjectileDto dto)
        {
            if (!string.Equals(dto.type, ShockOverloadSecondaryType, System.StringComparison.Ordinal))
            {
                return false;
            }

            GameObject prefab = Resources.Load<GameObject>(ShockOverloadPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Projectile prefab not found: {dto.type}");
                return true;
            }

            Vector3 position = ProjectileUtil.GetPosition(dto.start);
            GameObject effect = Instantiate(prefab, position, prefab.transform.rotation);
            SpriteRenderer renderer = effect.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.transform.localScale *= ShockOverloadSecondaryScale;
            }

            Destroy(effect, dto.duration);
            return true;
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

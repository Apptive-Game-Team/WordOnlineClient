using GameScene.Dto.Projectile;
using UnityEngine;

namespace GameScene.Object.Projectile
{
    public class StaticProjectile : MonoBehaviour, IProjectile
    {
        public void Init(ProjectileDto projectileDto)
        {
            transform.position = ProjectileUtil.GetPosition(projectileDto.start);
            transform.rotation = ProjectileUtil.GetRotation(projectileDto);
            
            Destroy(gameObject, projectileDto.duration);
        }
    }
}
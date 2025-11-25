
using Script.GameScene.Dto.Projectile;
using UnityEngine;

namespace Script.GameScene.Object.Projectile
{
    public interface IProjectile
    {

        public void Init(ProjectileDto projectileDto);
    }
}
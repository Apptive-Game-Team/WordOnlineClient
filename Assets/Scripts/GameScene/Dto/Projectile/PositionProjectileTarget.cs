using UnityEngine;

namespace GameScene.Dto.Projectile
{
    [System.Serializable]
    public class PositionProjectileTarget : ProjectileTarget
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}

using UnityEngine;

namespace Script.GameScene.Dto.Projectile
{
    [System.Serializable]
    public class ProjectileTarget
    {
        public string targetType;
        public int id;
        public float x, y, z;
        
        public Vector3 GetVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
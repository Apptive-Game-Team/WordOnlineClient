using Data;
using GameScene.Dto.Projectile;

namespace GameScene.Dto
{
    [System.Serializable]
    public class ObjectsInfo
    {
        public CreatedObjectDto[] create;
        public UpdatedObjectDto[] update;
        public ProjectileDto[] projectile;
    }
}
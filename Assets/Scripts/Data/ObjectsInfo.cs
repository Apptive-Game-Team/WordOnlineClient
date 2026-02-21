using GameScene.Dto.Projectile;

namespace Data
{
    [System.Serializable]
    public class ObjectsInfo
    {
        public CreatedObjectDto[] create;
        public UpdatedObjectDto[] update;
        public ProjectileDto[] projectile;
    }
}
using Scripts.GameScene.Dto.Projectile;

namespace Scripts.Data
{
    [System.Serializable]
    public class ObjectsInfo
    {
        public CreatedObjectDto[] create;
        public UpdatedObjectDto[] update;
        public ProjectileDto[] projectile;
    }
}
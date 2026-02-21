namespace Scripts.GameScene.Dto.Projectile
{
    [System.Serializable]
    public class ProjectileDto
    {
        public string type;
        public float duration;
        public ProjectileTarget start;
        public ProjectileTarget end;
    }
}
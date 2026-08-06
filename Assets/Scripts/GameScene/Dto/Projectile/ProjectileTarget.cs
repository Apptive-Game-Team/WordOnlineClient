using Newtonsoft.Json;

namespace GameScene.Dto.Projectile
{
    /// <summary>
    /// Where a projectile starts or ends. Mirrors the server hierarchy: either a fixed world position
    /// or a reference to a live object.
    /// </summary>
    [JsonConverter(typeof(ProjectileTargetConverter))]
    public abstract class ProjectileTarget
    {
        public string targetType;
    }
}

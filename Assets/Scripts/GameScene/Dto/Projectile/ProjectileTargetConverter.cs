using System;
using System.Collections.Generic;
using Global.Serialization;

namespace GameScene.Dto.Projectile
{
    public sealed class ProjectileTargetConverter : JsonSubtypeConverter<ProjectileTarget>
    {
        private static readonly IReadOnlyDictionary<string, Type> Subtypes = new Dictionary<string, Type>
        {
            { "position", typeof(PositionProjectileTarget) },
            { "reference", typeof(ReferenceProjectileTarget) }
        };

        public ProjectileTargetConverter() : base("targetType", Subtypes)
        {
        }
    }
}

using System;
using System.Collections.Generic;

namespace GameScene.Dto
{
    public static class StormStagChargeImpactRules
    {
        public const string EffectResourceName = "StormStagChargeImpact";
        public const string ReplacedProjectileType = "ElectricShot";

        private const string ChargeTier3 = "StormStagCharge3";
        private const string ChargeTier4 = "StormStagCharge4";

        public static bool ShouldPlay(IReadOnlyCollection<string> activeEffects)
        {
            if (activeEffects == null)
            {
                return false;
            }

            foreach (string effect in activeEffects)
            {
                if (string.Equals(effect, ChargeTier3, StringComparison.Ordinal)
                    || string.Equals(effect, ChargeTier4, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ShouldSuppressProjectile(
            string projectileType,
            IReadOnlyCollection<string> sourceActiveEffects)
        {
            return string.Equals(projectileType, ReplacedProjectileType, StringComparison.Ordinal)
                && ShouldPlay(sourceActiveEffects);
        }
    }
}

using System.Collections.Generic;

namespace Simulation.Core
{
    public static class SimEffectImmuneChart
    {
        private static readonly Dictionary<Effect, ElementType[]> Immunity = new()
        {
            { Effect.Burn, new[] { ElementType.FIRE } },
            { Effect.Wet, new[] { ElementType.WATER } },
            { Effect.Shock, new[] { ElementType.LIGHTNING } },
            { Effect.Snared, new[] { ElementType.NATURE } },
        };

        public static bool IsImmuneTo(SimGameObject obj, Effect effect)
        {
            if (!Immunity.TryGetValue(effect, out var blockers)) return false;
            foreach (var b in blockers)
                if (obj.Element.Has(b)) return true;
            return false;
        }
    }
}

using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Static element effectiveness matrix. All values stored as Fix64 for determinism.
    /// Row = attacker element, Column = defender element.
    /// Order: NONE, FIRE, NATURE, WATER, LIGHTNING, ROCK, WIND
    /// </summary>
    public static class ElementalChart
    {
        private static readonly Fix64 H = Fix64.Half;                    // 0.5
        private static readonly Fix64 O = Fix64.One;                     // 1.0
        private static readonly Fix64 D = Fix64.Two;                     // 2.0
        private static readonly Fix64 R = Fix64.FromDouble(1.5);         // 1.5

        // [atk][def]
        private static readonly Fix64[][] Chart =
        {
            //              NONE  FIRE  NAT   WAT   LGHT  ROCK  WIND
            /* NONE */  new[]{ O,  O,    O,    O,    O,    O,    O },
            /* FIRE */  new[]{ O,  O,    H,    D,    O,    O,    O },
            /* NAT  */  new[]{ O,  D,    O,    H,    O,    O,    O },
            /* WAT  */  new[]{ O,  H,    D,    O,    D,    H,    O },
            /* LGHT */  new[]{ O,  O,    O,    O,    O,    O,    O },
            /* ROCK */  new[]{ O,  H,    R,    R,    H,    R,    H },
            /* WIND */  new[]{ O,  O,    O,    O,    D,    D,    O },
        };

        public static Fix64 GetMultiplier(ElementType atk, ElementType def)
        {
            return Chart[(int)atk][(int)def];
        }

        public static Fix64 ComputePairwiseProductMultiplier(
            ICollection<ElementType> atkElements,
            ICollection<ElementType> defElements)
        {
            if (atkElements == null || atkElements.Count == 0)
                atkElements = new[] { ElementType.NONE };
            if (defElements == null || defElements.Count == 0)
                defElements = new[] { ElementType.NONE };

            Fix64 sum = Fix64.Zero;
            int count = 0;

            foreach (var atk in atkElements)
            {
                foreach (var def in defElements)
                {
                    sum = sum + GetMultiplier(atk, def);
                    count++;
                }
            }

            if (count == 0) return Fix64.One;

            return sum / Fix64.FromInt(count);
        }
    }
}

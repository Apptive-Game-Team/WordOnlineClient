using System.Collections.Generic;

namespace Simulation.Core
{
    public class AttackInfo
    {
        public int Damage;
        public HashSet<ElementType> Elements;
        public Effect Effect;

        public AttackInfo(int damage, ElementType element)
        {
            Damage = damage;
            Elements = new HashSet<ElementType> { element };
            Effect = Effect.None;
        }

        public AttackInfo(int damage, HashSet<ElementType> elements)
        {
            Damage = damage;
            Elements = elements;
            Effect = Effect.None;
        }
    }
}

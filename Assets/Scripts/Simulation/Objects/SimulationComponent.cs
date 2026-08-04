using System;
using FixMath.NET;
using GameScene.Simulation.Core;

namespace GameScene.Simulation.Objects
{
    [Flags]
    public enum SimulationElement : byte
    {
        None = 0,
        Fire = 1,
        Nature = 2,
        Water = 4,
        Lightning = 8,
        Rock = 16,
        Wind = 32
    }

    public static class SimulationElementalChart
    {
        public static int ScaleDamage(int damage, SimulationElement attack, SimulationElement defense)
        {
            const int referenceHealth = 1000000;
            return referenceHealth - ApplyToHealth(referenceHealth, damage, attack, defense);
        }

        public static int ApplyToHealth(int currentHealth, int damage,
            SimulationElement attack, SimulationElement defense)
        {
            SimulationElement[] attacks = Expand(attack);
            SimulationElement[] defenses = Expand(defense);
            Fix64 total = Fix64.Zero;
            for (int left = 0; left < attacks.Length; left++)
                for (int right = 0; right < defenses.Length; right++)
                    total += Multiplier(attacks[left], defenses[right]);
            Fix64 average = total / (Fix64)(attacks.Length * defenses.Length);
            return checked((int)((Fix64)currentHealth - damage * average));
        }

        private static SimulationElement[] Expand(SimulationElement value)
        {
            if (value == SimulationElement.None) return new[] { SimulationElement.None };
            SimulationElement[] all =
            {
                SimulationElement.Fire, SimulationElement.Nature, SimulationElement.Water,
                SimulationElement.Lightning, SimulationElement.Rock, SimulationElement.Wind
            };
            int count = 0;
            for (int index = 0; index < all.Length; index++) if ((value & all[index]) != 0) count++;
            SimulationElement[] result = new SimulationElement[count];
            int write = 0;
            for (int index = 0; index < all.Length; index++)
                if ((value & all[index]) != 0) result[write++] = all[index];
            return result;
        }

        private static Fix64 Multiplier(SimulationElement attack, SimulationElement defense)
        {
            if (attack == SimulationElement.None || defense == SimulationElement.None) return Fix64.One;
            switch (attack)
            {
                case SimulationElement.Fire:
                    if (defense == SimulationElement.Nature) return (Fix64)0.5m;
                    if (defense == SimulationElement.Water) return (Fix64)2;
                    return Fix64.One;
                case SimulationElement.Nature:
                    if (defense == SimulationElement.Fire) return (Fix64)2;
                    if (defense == SimulationElement.Water) return (Fix64)0.5m;
                    return Fix64.One;
                case SimulationElement.Water:
                    if (defense == SimulationElement.Fire || defense == SimulationElement.Rock)
                        return (Fix64)0.5m;
                    if (defense == SimulationElement.Nature || defense == SimulationElement.Lightning)
                        return (Fix64)2;
                    return Fix64.One;
                case SimulationElement.Lightning: return Fix64.One;
                case SimulationElement.Rock:
                    if (defense == SimulationElement.Fire || defense == SimulationElement.Lightning ||
                        defense == SimulationElement.Wind) return (Fix64)0.5m;
                    if (defense == SimulationElement.Nature || defense == SimulationElement.Water ||
                        defense == SimulationElement.Rock) return (Fix64)1.5m;
                    return Fix64.One;
                case SimulationElement.Wind:
                    if (defense == SimulationElement.Lightning || defense == SimulationElement.Rock)
                        return (Fix64)2;
                    return Fix64.One;
                default: throw new ArgumentOutOfRangeException(nameof(attack));
            }
        }
    }

    public sealed class SimulationComponent
    {
        public string Id { get; }
        public int SpawnFrame { get; }
        public int DestroyFrame { get; private set; } = -1;
        public bool IsDestroyed => DestroyFrame >= 0;

        internal SimulationComponent(string id, int spawnFrame)
        {
            Id = id;
            SpawnFrame = spawnFrame;
        }

        internal void Destroy(int frameNumber)
        {
            if (!IsDestroyed) DestroyFrame = frameNumber;
        }

        internal void WriteState(CanonicalStateWriter writer)
        {
            writer.WriteString(Id);
            writer.WriteInt32(SpawnFrame);
            writer.WriteInt32(DestroyFrame);
        }
    }
}

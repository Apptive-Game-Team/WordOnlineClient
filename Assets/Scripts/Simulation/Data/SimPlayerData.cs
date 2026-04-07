using System.Collections.Generic;

namespace Simulation.Core
{
    public class SimPlayerData
    {
        public const int MAX_CARD_NUM = 6;
        public const int MAX_HP = 100;

        public Fix64 Mana;
        public Fix64 MaxMana;
        public Fix64 ManaRegenRate;
        public int Hp = MAX_HP;
        public readonly List<SimCardType> Cards = new();
        public event Action<SimCardType> OnCardAdded;

        private readonly Dictionary<string, Dictionary<string, Fix64>> _parameters;

        public SimPlayerData(Dictionary<string, Dictionary<string, Fix64>> parameters)
        {
            _parameters = parameters;

            // Load mana parameters
            if (_parameters.TryGetValue("player", out var p))
            {
                p.TryGetValue("max_mana", out MaxMana);
                p.TryGetValue("mana_regen_rate", out ManaRegenRate);
            }

            if (MaxMana == Fix64.Zero) MaxMana = Fix64.FromInt(10);
            if (ManaRegenRate == Fix64.Zero) ManaRegenRate = Fix64.FromInt(1); // 1.0 mana per second

            Mana = MaxMana / Fix64.FromInt(2); // Start with half mana
        }

        public void RegenerateMana(Fix64 deltaTime)
        {
            Mana += ManaRegenRate * deltaTime;
            if (Mana > MaxMana) Mana = MaxMana;
        }

        public bool AddCard(SimCardType card)
        {
            if (Cards.Count >= MAX_CARD_NUM) return false;
            Cards.Add(card);
            OnCardAdded?.Invoke(card);
            return true;
        }

        public bool UseCards(List<SimCardType> cards)
        {
            Fix64 totalManaCost = Fix64.Zero;
            var temp = new List<SimCardType>(Cards);
            foreach (var card in cards)
            {
                string key = card.ToString().ToLower();
                if (_parameters.TryGetValue(key, out var p) && p.TryGetValue("mana_cost", out var cost))
                    totalManaCost += cost;
                if (!temp.Remove(card)) return false;
            }
            if (totalManaCost > Mana) return false;

            foreach (var card in cards)
            {
                Cards.Remove(card);
                string key = card.ToString().ToLower();
                if (_parameters.TryGetValue(key, out var p) && p.TryGetValue("mana_cost", out var cost))
                    Mana -= cost;
            }
            return true;
        }
    }
}

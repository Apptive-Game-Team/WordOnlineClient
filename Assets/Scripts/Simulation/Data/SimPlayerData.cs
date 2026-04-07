using System.Collections.Generic;

namespace Simulation.Core
{
    public class SimPlayerData
    {
        public const int MAX_CARD_NUM = 6;
        public const int MAX_HP = 100;

        public int Mana;
        public int Hp = MAX_HP;
        public readonly List<SimCardType> Cards = new();

        private readonly Dictionary<string, Dictionary<string, Fix64>> _parameters;

        public SimPlayerData(Dictionary<string, Dictionary<string, Fix64>> parameters)
        {
            _parameters = parameters;
        }

        public bool AddCard(SimCardType card)
        {
            if (Cards.Count >= MAX_CARD_NUM) return false;
            Cards.Add(card);
            return true;
        }

        public bool UseCards(List<SimCardType> cards)
        {
            int totalManaCost = 0;
            var temp = new List<SimCardType>(Cards);
            foreach (var card in cards)
            {
                string key = card.ToString().ToLower();
                if (_parameters.TryGetValue(key, out var p) && p.TryGetValue("mana_cost", out var cost))
                    totalManaCost += cost.ToInt();
                if (!temp.Remove(card)) return false;
            }
            if (totalManaCost > Mana) return false;

            foreach (var card in cards)
            {
                Cards.Remove(card);
                string key = card.ToString().ToLower();
                if (_parameters.TryGetValue(key, out var p) && p.TryGetValue("mana_cost", out var cost))
                    Mana -= cost.ToInt();
            }
            return true;
        }
    }
}

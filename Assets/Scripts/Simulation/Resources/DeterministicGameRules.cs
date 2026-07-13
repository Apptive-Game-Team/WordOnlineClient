using System;
using System.Collections.Generic;

namespace GameScene.Simulation.Resources
{
    public sealed class DeterministicGameRules
    {
        private readonly Dictionary<string, int> manaCosts;
        private readonly HashSet<string> magicRecipes;

        public int MaxMana { get; }
        public int MaxHandSize { get; }
        public int ManaChargeIntervalFrames { get; }
        public int ManaChargeAmount { get; }
        public int CardDrawIntervalFrames { get; }
        public int DurationFrames { get; }

        public DeterministicGameRules(int maxMana, int maxHandSize, int manaChargeIntervalFrames,
            int manaChargeAmount, int cardDrawIntervalFrames, int durationFrames,
            IReadOnlyDictionary<string, int> manaCosts, IEnumerable<IEnumerable<string>> magicRecipes)
        {
            if (maxMana < 0 || maxHandSize < 1 || manaChargeIntervalFrames < 1
                || manaChargeAmount < 0 || cardDrawIntervalFrames < 1 || durationFrames < 1)
                throw new ArgumentOutOfRangeException(nameof(maxMana));
            MaxMana = maxMana;
            MaxHandSize = maxHandSize;
            ManaChargeIntervalFrames = manaChargeIntervalFrames;
            ManaChargeAmount = manaChargeAmount;
            CardDrawIntervalFrames = cardDrawIntervalFrames;
            DurationFrames = durationFrames;
            this.manaCosts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, int> pair in manaCosts)
            {
                if (pair.Value < 0) throw new ArgumentOutOfRangeException(nameof(manaCosts));
                this.manaCosts.Add(pair.Key, pair.Value);
            }
            this.magicRecipes = new HashSet<string>(StringComparer.Ordinal);
            foreach (IEnumerable<string> recipe in magicRecipes) this.magicRecipes.Add(RecipeKey(recipe));
        }

        public int ManaCost(string card) => manaCosts.TryGetValue(card, out int cost)
            ? cost : throw new InvalidOperationException("Missing mana cost for card: " + card);
        public bool IsMagic(IEnumerable<string> cards) => magicRecipes.Contains(RecipeKey(cards));

        private static string RecipeKey(IEnumerable<string> cards)
        {
            List<string> ordered = new List<string>(cards);
            ordered.Sort(StringComparer.Ordinal);
            return string.Join("\u001f", ordered);
        }
    }
}

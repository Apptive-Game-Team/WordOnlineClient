using System.Collections.Generic;
using Data;

namespace GameScene.Card
{
    public static class MagicSuggestion
    {
        public static List<CombinedMagicData> GetAvailableByHand(IList<CardType> hand)
        {
            var handCount = new Dictionary<CardType, int>();
            foreach (var card in hand)
            {
                if (!handCount.TryAdd(card, 1))
                    handCount[card]++;
            }

            var result = new List<CombinedMagicData>();

            foreach (var data in LocalCombinedMagicData.dataList)
            {
                if (CanMake(data.recipe, handCount))
                    result.Add(data);
            }

            return result;
        }
        
        public static List<CombinedMagicData> PickTopN(IList<CardType> hand, int count)
        {
            var list = GetAvailableByHand(hand);
            
            list.Sort((a, b) => b.recipe.Count.CompareTo(a.recipe.Count));

            if (list.Count > count)
                list.RemoveRange(count, list.Count - count);

            return list;
        }

        private static bool CanMake(IList<CardType> recipe, Dictionary<CardType, int> handCount)
        {
            var need = new Dictionary<CardType, int>();
            foreach (var t in recipe)
            {
                if (!need.TryAdd(t, 1))
                    need[t]++;
            }

            foreach (var kv in need)
            {
                if (!handCount.TryGetValue(kv.Key, out var have))
                    return false;
                if (have < kv.Value)
                    return false;
            }

            return true;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Data.Magic
{
    public class CombinedMagicResolver : MonoBehaviour
    {
        private List<CombinedMagicData> dataList;
        [SerializeField] private UserMagicService userMagicService;

        private void Awake()
        {
            userMagicService = FindObjectOfType<UserMagicService>();
            userMagicService.GetCombinedMagicData(list => dataList = list);
        }

        public bool CanResolve(IList<CardType> recipe)
        {
            foreach (var d in dataList)
            {
                if (AreSameMultiset(d.recipe, recipe))
                {
                    return true;
                }
            }
            return false;
        }
    
        public bool TryResolve(IList<CardType> recipe, out CombinedMagicData data)
        {
            foreach (var d in dataList)
            {
                if (AreSameMultiset(d.recipe, recipe))
                {
                    data = d;
                    return true;
                }
            }
            data = null;
            return false;
        }
    
        private static bool AreSameMultiset(IList<CardType> a, IList<CardType> b)
        {
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;

            var counts = new Dictionary<CardType, int>();
            foreach (var x in a)
            {
                counts.TryGetValue(x, out var c);
                counts[x] = c + 1;
            }

            foreach (var y in b)
            {
                if (!counts.TryGetValue(y, out var c) || c == 0)
                    return false;
                counts[y] = c - 1;
            }

            return true;
        }
    }
}
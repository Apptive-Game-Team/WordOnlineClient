using System.Collections.Generic;
using UnityEngine;

namespace Data.Magic
{
    public class CombinedMagicResolver : MonoBehaviour
    {
        private List<CombinedMagicData> dataList = new();
        [SerializeField] private UserMagicService userMagicService;

        private void Awake()
        {
            userMagicService ??= FindObjectOfType<UserMagicService>();
            if (userMagicService == null)
            {
                Debug.LogWarning("[CombinedMagicResolver] UserMagicService was not found.");
                return;
            }

            userMagicService.GetCombinedMagicData(list => dataList = list ?? new List<CombinedMagicData>());
        }

        public bool CanResolve(IList<CardType> recipe)
        {
            if (recipe == null || dataList == null || dataList.Count == 0)
            {
                return false;
            }

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
            if (recipe == null || dataList == null || dataList.Count == 0)
            {
                data = null;
                return false;
            }

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
    
        // CardType은 값이 촘촘한 작은 enum이라 Dictionary 대신 고정 배열로 센다.
        // TryResolve가 필드 선택 중 매 프레임, dataList 항목마다 호출되므로 여기서 할당이 생기면 안 된다.
        private static readonly int[] RecipeCounts = new int[System.Enum.GetValues(typeof(CardType)).Length];

        private static bool AreSameMultiset(IList<CardType> a, IList<CardType> b)
        {
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;

            System.Array.Clear(RecipeCounts, 0, RecipeCounts.Length);

            for (int i = 0; i < a.Count; i++)
            {
                int index = (int)a[i];
                if (index < 0 || index >= RecipeCounts.Length) return false;
                RecipeCounts[index]++;
            }

            for (int i = 0; i < b.Count; i++)
            {
                int index = (int)b[i];
                if (index < 0 || index >= RecipeCounts.Length || RecipeCounts[index] == 0) return false;
                RecipeCounts[index]--;
            }

            return true;
        }
    }
}

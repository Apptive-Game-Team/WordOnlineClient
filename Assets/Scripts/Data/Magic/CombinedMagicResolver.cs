using System.Collections.Generic;
using UnityEngine;

namespace Data.Magic
{
    /// <summary>
    /// 보유 조합 마법 목록의 로드 상태.
    /// "아직 안 옴"과 "받았는데 일치하는 레시피가 없음"은 호출자가 반드시 구분해야 한다.
    /// </summary>
    public enum MagicRecipeLoadState
    {
        Loading,
        Loaded,
        Failed
    }

    public class CombinedMagicResolver : MonoBehaviour
    {
        private List<CombinedMagicData> dataList = new();
        [SerializeField] private UserMagicService userMagicService;

        /// <summary>레시피 목록 로드 상태.</summary>
        public MagicRecipeLoadState LoadState { get; private set; } = MagicRecipeLoadState.Loading;

        /// <summary>
        /// <see cref="CanResolve"/>와 <see cref="TryResolve"/>의 false를 "일치하는 레시피 없음"으로
        /// 해석해도 되는 상태인지. 로드 전이거나 로드에 실패했으면 false다.
        /// </summary>
        public bool IsRecipeDataReady => LoadState == MagicRecipeLoadState.Loaded;

        private void Awake()
        {
            userMagicService ??= FindObjectOfType<UserMagicService>();
            if (userMagicService == null)
            {
                Debug.LogWarning("[CombinedMagicResolver] UserMagicService was not found.");
                LoadState = MagicRecipeLoadState.Failed;
                return;
            }

            RequestLoad();
        }

        /// <summary>
        /// 로드에 실패한 상태에서만 다시 요청한다.
        /// 실패가 영구 상태로 굳으면 그 판에서는 마법 확정 자체가 막히기 때문이다.
        /// </summary>
        public void RequestReloadIfFailed()
        {
            if (LoadState != MagicRecipeLoadState.Failed || userMagicService == null)
            {
                return;
            }

            RequestLoad();
        }

        private void RequestLoad()
        {
            LoadState = MagicRecipeLoadState.Loading;
            userMagicService.GetCombinedMagicData(list =>
            {
                if (list == null)
                {
                    Debug.LogWarning("[CombinedMagicResolver] Failed to load combined magic data.");
                    dataList = new List<CombinedMagicData>();
                    LoadState = MagicRecipeLoadState.Failed;
                    return;
                }

                dataList = list;
                LoadState = MagicRecipeLoadState.Loaded;
            });
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

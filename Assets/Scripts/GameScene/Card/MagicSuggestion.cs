using System.Collections.Generic;
using Data.Magic;
using UnityEngine;

namespace GameScene.Card
{
    /// <summary>
    /// 손패에서 시전할 수 있는 마법을 고른다.
    /// 카드 한 장이 곧 마법 하나가 되어 손패의 모든 카드가 바로 시전 가능하므로,
    /// 이제 이 클래스는 손패를 그대로 돌려주는 것 이상을 하지 않는다.
    /// TODO(#576): 조합 추천 UI 자체가 없어진다. 이 클래스와 MagicHelperUI, MagicSuggestionItemView 를
    /// 지우고 GameScene 의 추천 목록 UI 도 함께 걷어낸다.
    /// </summary>
    public class MagicSuggestion : MonoBehaviour
    {
        [SerializeField] private UserMagicService userMagicService;

        private List<CombinedMagicData> ownedMagics = new();

        private void Awake()
        {
            if (userMagicService == null)
            {
                return;
            }

            userMagicService.GetCombinedMagicData((list) =>
            {
                if (list == null)
                {
                    Debug.LogError("Failed to load owned magic data");
                }

                ownedMagics = list ?? new List<CombinedMagicData>();
            });
        }

        /// <summary>손패에 있는 마법 중 보유한 것만 돌려준다.</summary>
        public List<CombinedMagicData> GetAvailableByHand(IList<CombinedMagicData> hand)
        {
            var result = new List<CombinedMagicData>();
            if (hand == null)
            {
                return result;
            }

            foreach (CombinedMagicData magic in hand)
            {
                if (magic == null || result.Contains(magic))
                {
                    continue;
                }

                if (ownedMagics.Count == 0 || ownedMagics.Exists(owned => owned.id == magic.id))
                {
                    result.Add(magic);
                }
            }

            return result;
        }

        public List<CombinedMagicData> PickTopN(IList<CombinedMagicData> hand, int count)
        {
            var list = GetAvailableByHand(hand);

            // 마나가 비싼 것이 대체로 센 마법이므로 그것부터 권한다.
            list.Sort((a, b) => b.manaCost.CompareTo(a.manaCost));

            if (list.Count > count)
            {
                list.RemoveRange(count, list.Count - count);
            }

            return list;
        }
    }
}

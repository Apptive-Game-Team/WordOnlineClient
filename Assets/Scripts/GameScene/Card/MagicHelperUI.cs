using System.Collections.Generic;
using Data.Magic;
using UnityEngine;

namespace GameScene.Card
{
    public class MagicHelperUI : MonoBehaviour
    {
        [Header("Hand")]
        [SerializeField] private Transform handRoot; 

        [Header("Suggestion UI")]
        [SerializeField] private Transform suggestionRoot;
        [SerializeField] private MagicSuggestionItemView suggestionItemPrefab;

        [SerializeField] private MagicSuggestion magicSuggestion;
        
        private readonly List<MagicSuggestionItemView> _spawnedItems = new();

        /// <summary>훈수 시스템이 강조할 추천 목록 루트.</summary>
        public Transform SuggestionRoot => suggestionRoot;

        /// <summary>
        /// 추천을 새로 뽑고 가장 위 후보의 재료 카드를 강조한다. 훈수가 마법 실패나 미사용을
        /// 짚을 때 쓰며, 유저가 추천을 직접 눌렀을 때와 같은 강조 경로를 탄다.
        /// </summary>
        public bool TryHighlightTopSuggestion()
        {
            RefreshSuggestions();

            if (_spawnedItems.Count == 0)
            {
                return false;
            }

            var candidates = magicSuggestion.PickTopN(GetHandMagics(), 1);
            if (candidates.Count == 0)
            {
                return false;
            }

            OnSuggestionClicked(candidates[0]);
            return true;
        }

        /// <summary>손패 강조를 모두 끈다.</summary>
        public void ClearHandHighlight()
        {
            foreach (var card in handRoot.GetComponentsInChildren<CardUI>())
            {
                card.SetHighlighted(false);
            }
        }

        public void RefreshSuggestions()
        {
            var candidates = magicSuggestion.PickTopN(GetHandMagics(), 3);
            
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            _spawnedItems.Clear();

            foreach (var data in candidates)
            {
                var item = Instantiate(suggestionItemPrefab, suggestionRoot);
                item.Setup(data, this);
                _spawnedItems.Add(item);
            }
        }
        
        /// <summary>권한 마법과 같은 카드 한 장만 손패에서 강조한다.</summary>
        public void OnSuggestionClicked(CombinedMagicData data)
        {
            var handCards = handRoot.GetComponentsInChildren<CardUI>();

            foreach (var card in handCards)
            {
                card.SetHighlighted(false);
            }

            if (data == null)
            {
                return;
            }

            foreach (var card in handCards)
            {
                if (card.Magic != null && card.Magic.id == data.id)
                {
                    card.SetHighlighted(true);
                    return;
                }
            }
        }

        private List<CombinedMagicData> GetHandMagics()
        {
            var handCards = handRoot.GetComponentsInChildren<CardUI>();
            var magics = new List<CombinedMagicData>(handCards.Length);
            foreach (var card in handCards)
            {
                if (card.Magic != null)
                {
                    magics.Add(card.Magic);
                }
            }

            return magics;
        }
    }
}

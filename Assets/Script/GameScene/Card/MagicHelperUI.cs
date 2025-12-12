using System.Collections.Generic;
using Script.Data;
using UnityEngine;

namespace Script.GameScene
{
    public class MagicHelperUI : MonoBehaviour
    {
        [Header("Hand")]
        [SerializeField] private Transform handRoot; 

        [Header("Suggestion UI")]
        [SerializeField] private Transform suggestionRoot;
        [SerializeField] private MagicSuggestionItemView suggestionItemPrefab;

        private readonly List<MagicSuggestionItemView> _spawnedItems = new();
        
        public void RefreshSuggestions()
        {
            var handCards = handRoot.GetComponentsInChildren<CardUI>();
            var handTypes = new List<CardType>(handCards.Length);
            foreach (var card in handCards)
            {
                handTypes.Add(card.CardType);
            }
            
            var candidates = MagicSuggestion.PickTopN(handTypes, 3);
            
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
        
        public void OnSuggestionClicked(CombinedMagicData data)
        {
            var handCards = handRoot.GetComponentsInChildren<CardUI>();
            
            foreach (var card in handCards)
                card.SetHighlighted(false);
            
            var need = new Dictionary<CardType, int>();
            foreach (var t in data.recipe)
            {
                if (!need.TryAdd(t, 1))
                    need[t]++;
            }
            
            var used = new Dictionary<CardType, int>();
            
            foreach (var card in handCards)
            {
                var type = card.CardType;

                if (!need.TryGetValue(type, out var needCount))
                    continue;

                used.TryGetValue(type, out var usedCount);
                if (usedCount >= needCount)
                    continue;

                card.SetHighlighted(true);
                used[type] = usedCount + 1;
            }
        }
    }
}

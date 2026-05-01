using System.Collections.Generic;
using Data;
using Data.Magic;
using GameScene.Card;
using Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene
{
    public class GameSceneUIController : LocalSingletonObject<GameSceneUIController>
    {
        [SerializeField] private CombinedMagicResolver combinedMagicResolver;
        
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private Slider manaSlider;
    
        [SerializeField] private CardUI cardUIPrefab;
        [SerializeField] private GameObject lowerBar;

        [SerializeField] private CardImageMapper cardImageMapper;

        [SerializeField] private ExpectedMagicUI expectedMagicUI;
    
        [SerializeField] private MagicHelperUI magicHelperUI;
        
        [SerializeField] private Sprite expectingFailedMagicImage;
        
        public void UpdateMana(int mana)
        {
            if (manaText == null || manaSlider == null) return;
            manaText.text = mana.ToString();
            manaSlider.value = mana;
        }

        public void AddCard(string cardname)
        {
            if (lowerBar == null || cardUIPrefab == null || magicHelperUI == null || cardImageMapper == null) return;
            CardUI cardUI = Instantiate(cardUIPrefab, lowerBar.transform);
            cardUI.Init(cardname, cardImageMapper.GetCardImage(cardname));
            magicHelperUI.RefreshSuggestions();
        }

        public void RemoveCard(string cardName)
        {
            foreach (Transform child in lowerBar.transform)
            {
                CardUI cardUI = child.GetComponent<CardUI>();
                if (cardUI != null && cardUI.CardName == cardName)
                {
                    Destroy(child.gameObject);
                    magicHelperUI.RefreshSuggestions();
                    return;
                }
            }
        }

        public List<string> GetAllCards()
        {
            if (lowerBar == null) return new List<string>();
            List<string> cardNames = new List<string>();
            foreach (Transform child in lowerBar.transform)
            {
                cardNames.Add(child.GetComponent<CardUI>().CardName);
            }
            return cardNames;
        } 

        public void TrySetExpectedMagicUI(IList<CardType> recipe)
        {
            if (expectedMagicUI == null) return;
            combinedMagicResolver.TryResolve(recipe, out CombinedMagicData data);
            if (data != null)
            {
                expectedMagicUI.SetImage(data.GetSprite());
                return;
            }

            if (recipe.Count == 0)
            {
                expectedMagicUI.SetImage(null);
            }
            else
            {
                expectedMagicUI.SetImage(expectingFailedMagicImage);
            }
        }
    }
}

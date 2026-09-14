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
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private Slider manaSlider;
    
        [SerializeField] private CardUI cardUIPrefab;
        [SerializeField] private GameObject lowerBar;

        [SerializeField] private ExpectedMagicUI expectedMagicUI;
    
        [SerializeField] private MagicHelperUI magicHelperUI;
        
        [SerializeField] private Sprite expectingFailedMagicImage;
        
        private ManaCostPreview manaCostPreview;
        private int currentMana;
        private int expectedManaCost;

        public void UpdateMana(int mana)
        {
            if (manaText == null || manaSlider == null) return;
            manaText.text = mana.ToString();
            manaSlider.value = mana;
            currentMana = mana;
            RefreshManaCostPreview();
        }

        /// <summary>
        /// 준비 중인 조합이 먹을 마나를 마나 바에 덧그린다. 조합이 비거나 시전 입력을 보낸 뒤에는
        /// 0을 넣어 덧그리기를 지운다.
        /// </summary>
        public void SetExpectedManaCost(int cost)
        {
            expectedManaCost = cost;
            RefreshManaCostPreview();
        }

        private void RefreshManaCostPreview()
        {
            if (manaSlider == null) return;
            manaCostPreview ??= new ManaCostPreview(manaSlider);
            manaCostPreview.Render(currentMana, expectedManaCost);
        }

        /// <summary>
        /// 손패에 카드를 한 장 붙인다. 카드 앞면은 마법별 아트다.
        /// TODO(#576): 서버 frame 의 cards.added 가 이름 목록에서 마법 id 목록으로 바뀌면
        /// 이 인자를 long 으로 옮기고 id 로 마법을 찾는다.
        /// </summary>
        public void AddCard(string cardname)
        {
            if (lowerBar == null || cardUIPrefab == null || magicHelperUI == null) return;
            CardUI cardUI = Instantiate(cardUIPrefab, lowerBar.transform);
            cardUI.Init(cardname, DeckScene.DeckCardSpriteResolver.GetMagicSprite(cardname));
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

        public CardUI GetCardAt(int index)
        {
            if (lowerBar == null) return null;
            return CardHotkey.FindCardAt<CardUI>(lowerBar.transform, index);
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

        /// <summary>고른 카드의 마법을 수정구에 그린다. 고른 것이 없으면 비운다.</summary>
        public void TrySetExpectedMagicUI(CombinedMagicData magic, int selectedCardCount)
        {
            if (expectedMagicUI == null) return;
            if (magic != null)
            {
                expectedMagicUI.SetImage(magic.GetSprite());
                return;
            }

            expectedMagicUI.SetImage(selectedCardCount == 0 ? null : expectingFailedMagicImage);
        }
    }
}

using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Card
{
    public class ZoomedCardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text cardNameText;
        [SerializeField] private TMP_Text cardManaText;
        [SerializeField] private Image cardImage;
        
        public void Show(CardUI cardUI)
        {
            gameObject.SetActive(true);
            Vector3 position = gameObject.transform.position;
            position.x = cardUI.transform.position.x;
            gameObject.transform.position = position;
            cardManaText.text = cardUI.Mana;
            cardImage.sprite = cardUI.CardSprite;
            cardNameText.text = cardUI.DisplayName;
            gameObject.SetActive(true);
        }
    
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
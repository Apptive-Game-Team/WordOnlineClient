using Script.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GameScene
{
    public class ZoomedCardUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text cardNameText;
        [SerializeField] private TMP_Text cardManaText;
        [SerializeField] private Image cardBackgroundImage;
        [SerializeField] private Image cardImage;
        
        public void Show(CardUI cardUI)
        {
            gameObject.SetActive(true);
            MagicData magicData = LocalMagicData.GetMagicData(cardUI.CardName);
            Vector3 position = gameObject.transform.position;
            position.x = cardUI.transform.position.x;
            gameObject.transform.position = position;
            cardManaText.text = cardUI.Mana;
            cardBackgroundImage.sprite = cardUI.GetComponent<Image>().sprite;
            cardImage.sprite = cardUI.transform.GetChild(2).GetComponent<Image>().sprite;
            cardNameText.text = cardUI.DisplayName;
            gameObject.SetActive(true);
        }
    
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
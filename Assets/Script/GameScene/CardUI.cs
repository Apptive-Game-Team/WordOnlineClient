using System.Linq;
using Script.Data;
using Script.Data.Sound;
using Script.Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GameScene
{
    public class CardUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardManaText;
        [SerializeField] private AudioSource cardSound;
        
        [SerializeField] private Sprite typeSprite;
        [SerializeField] private Sprite magicSprite;
        private void Awake()
        {
            cardSound = gameObject.GetComponent<AudioSource>();
            if (cardSound == null)
            {
                cardSound = gameObject.AddComponent<AudioSource>();
            }
            cardSound.clip = SoundAssets.DrawCard;
            cardSound.volume = SoundData.gameVolume / 100f;
        }

        private bool isActive = false;
    
        public string CardName => cardNameText.text;

        public void Init(string name)
        {
            cardNameText.text = name;
            MagicData magicData = LocalMagicData.GetMagicData(name);
            cardManaText.text = magicData.mana.ToString();
            switch (magicData.type)
            {
                case "type":
                    GetComponent<Image>().sprite = typeSprite;
                    break;
                case "magic":
                    GetComponent<Image>().sprite = magicSprite;
                    break;
                default:
                    WDebug.LogError($"Unknown magic type: {magicData.type}");
                    break;
            }
        }

        public void SetCardActive(bool isActive)
        {
            this.isActive = isActive;
            GetComponent<Image>().color = isActive ? Color.gray : Color.white;
        }
        
        public void OnCardClicked()
        {
            cardSound.Play();
            CardInputSender cardInputSender = FindObjectOfType<CardInputSender>();
            if (isActive)
            {
                PlayerFeedbackController.Instance.CancelCardSelectFeedback();
                cardInputSender.CancelUseCard(this);
                SetCardActive(false);
            }
            else
            {
                PlayerFeedbackController.Instance.PlayCardSelectFeedback();
                cardInputSender.TryUseCard(this);   
                SetCardActive(true);
            }
            cardInputSender.SetExpectedMagicUI(); 
        }
    }
}
using System;
using Data;
using Data.Localization;
using Data.Magic;
using Data.Sound;
using Sound;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TutorialScene
{
    public class TutorialCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardManaText;
        [SerializeField] private AudioSource cardSound;
        
        [SerializeField] private Outline outline;
        
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
    
        public string CardName;
        public CardType CardType { get; private set; }
        public string DisplayName => cardNameText.text;
        public string Mana => cardManaText.text;

        public async void Init(string name)
        {
            CardName = name;
            MagicData magicData = LocalMagicData.GetMagicData(name);
            CardType = Enum.Parse<CardType>(name, true);
            cardManaText.text = magicData.mana.ToString();
            cardNameText.text = await LocaleUtils.GetStringAsync("Card", name);
        }

        public void SetCardActive(bool isActive)
        {
            this.isActive = isActive;
            GetComponent<Image>().color = isActive ? Color.gray : Color.white;
        }
        
        public void OnCardClicked()
        {
            cardSound.Play();
            TutorialCardSender cardInputSender = FindObjectOfType<TutorialCardSender>();
            if (isActive)
            {
                cardInputSender.CancelUseCard(this);
                SetCardActive(false);
            }
            else
            {
                cardInputSender.TryUseCard(this);   
                SetCardActive(true);
            }
            cardInputSender.SetExpectedMagicUI(); 
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
        public void SetHighlighted(bool on)
        {
            if (outline != null)
                outline.enabled = on;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }
    }
}
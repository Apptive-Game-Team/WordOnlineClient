using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GameScene
{
    public class CardUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private Sprite activeCardImage;
        [SerializeField] private Sprite inactiveCardImage;
        [SerializeField] private AudioSource cardSound;
        
        string[] typeArray = { "fire", "water", "lightning", "rock", "leaf" };
        string[] magicArray = { "shoot", "spawn", "summon", "explode" };
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
        }

        private bool isActive = false;
    
        public string CardName => cardNameText.text;

        public void Init(string name)
        {
            cardNameText.text = name;
            if (typeArray.Contains(name))
            {
                GetComponent<Image>().sprite = typeSprite;
            }
            else if (magicArray.Contains(name))
            {
                GetComponent<Image>().sprite = magicSprite;
            }
        }

        public void SetCardActive(bool isActive)
        {
            this.isActive = isActive;
            GetComponent<Image>().sprite = isActive ? activeCardImage : inactiveCardImage;
        }
        
        public void OnCardClicked()
        {
            cardSound.Play();   
            if (isActive)
            {
                FindObjectOfType<CardInputSender>().CancelUseCard(this);
                SetCardActive(false);
            }
            else
            {
                FindObjectOfType<CardInputSender>().TryUseCard(this);   
                GetComponent<Image>().sprite = activeCardImage;
                SetCardActive(true);
            }
        }
    }
}
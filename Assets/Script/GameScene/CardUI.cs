using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GameScene
{
    public class CardUI : MonoBehaviour
    {
        private static HashSet<String> MagicCards = new HashSet<String>
        {
            "shoot", "explode", "summon", "spawn",
        };
        private static HashSet<String> TypeCards = new HashSet<String>
        {
            "water", "fire", "lighting", "leaf", "rock",
        };
        
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private Sprite magicCardImage;
        [SerializeField] private Sprite typeCardImage;
        [SerializeField] private AudioSource cardSound;
        
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
            if (MagicCards.Contains(name.ToLower()))
            {
                GetComponent<Image>().sprite = magicCardImage;
            }
            else if (TypeCards.Contains(name.ToLower()))
            {
                GetComponent<Image>().sprite = typeCardImage;
            }
        }

        public void SetCardActive(bool isActive)
        {
            this.isActive = isActive;
            Color color = GetComponent<Image>().color;
            if (isActive)
            {
                color = Color.white * 0.5f;
                
            }
            else
            {
                color = Color.white;
            }
            GetComponent<Image>().color = color;
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
                SetCardActive(true);
            }
        }
    }
}
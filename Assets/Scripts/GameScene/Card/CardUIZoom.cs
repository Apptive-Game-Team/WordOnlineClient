using UnityEngine;
using Data.Sound;
using Sound;

namespace GameScene.Card
{
    public class CardUIZoom : MonoBehaviour
    {
        private const float HoverSoundCooldown = 0.12f;
        public static CardUIZoom Instance { get; private set; }
    
        [SerializeField] private ZoomedCardUI zoomedCardUI;
        [SerializeField] private AudioSource audioSource;
        private float lastHoverSoundTime = float.NegativeInfinity;
    
        private void Awake()
        {
            Instance = this;
            zoomedCardUI.Hide();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.clip = SoundAssets.TouchCard;
            audioSource.volume = SoundData.gameVolume / 100f;
        }
    
        public void Show(CardUI cardUI)
        {
            zoomedCardUI.Show(cardUI);
            if (Time.unscaledTime - lastHoverSoundTime >= HoverSoundCooldown)
            {
                audioSource.PlayOneShot(SoundAssets.TouchCard);
                lastHoverSoundTime = Time.unscaledTime;
            }
        }
    
        public void Hide()
        {
            zoomedCardUI.Hide();
        }
    }
}

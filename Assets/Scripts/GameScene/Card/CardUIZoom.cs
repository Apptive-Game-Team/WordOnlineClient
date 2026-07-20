using UnityEngine;
using Data.Sound;
using Sound;

namespace GameScene.Card
{
    public class CardUIZoom : MonoBehaviour
    {
        public static CardUIZoom Instance { get; private set; }
    
        [SerializeField] private ZoomedCardUI zoomedCardUI;
        [SerializeField] private AudioSource audioSource;
    
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
            audioSource.PlayOneShot(SoundAssets.TouchCard);
        }
    
        public void Hide()
        {
            zoomedCardUI.Hide();
        }
    }
}

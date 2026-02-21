using UnityEngine;

namespace Scripts.GameScene.Card
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
        }
    
        public void Show(CardUI cardUI)
        {
            zoomedCardUI.Show(cardUI);
            audioSource.Play();
        }
    
        public void Hide()
        {
            zoomedCardUI.Hide();
        }
    }
}

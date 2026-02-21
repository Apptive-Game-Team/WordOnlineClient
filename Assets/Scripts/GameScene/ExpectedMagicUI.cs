using UnityEngine;
using UnityEngine.UI;

namespace Scripts.GameScene
{
    public class ExpectedMagicUI : MonoBehaviour
    {
        [SerializeField] private Image imagePrefab;

        private void Awake()
        {
            imagePrefab.gameObject.SetActive(false);
        }

        public void SetImage(Sprite sprite)
        {
            if (sprite == null)
            {
                imagePrefab.gameObject.SetActive(false);
                return;
            }
            imagePrefab.gameObject.SetActive(true);
            imagePrefab.sprite = sprite;
        }
    }
}
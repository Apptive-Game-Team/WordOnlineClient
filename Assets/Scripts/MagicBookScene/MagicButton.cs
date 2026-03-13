using System;
using Data;
using Data.Localization;
using Data.Magic;
using Data.Util;
using UnityEngine;
using UnityEngine.UI;

namespace MagicBookScene
{
    public class MagicButton : MonoBehaviour
    {
        [SerializeField] private Image magicImage;
        
        private CombinedMagicData data;
        public event Action<CombinedMagicData> OnClick;
        
        private void OnButtonClick()
        {
            OnClick?.Invoke(data);
        }
        
        public void SetActive(bool active)
        {
            GetComponent<Button>().interactable = active;
            magicImage.color = active ? Color.white : new Color(0, 0, 0, 0.6f);
        }

        public void Init(CombinedMagicData data)
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
            this.data = data;
            magicImage.sprite = data.GetSprite();
        }
    }
}

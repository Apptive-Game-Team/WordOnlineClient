using System;
using Scripts.Data;
using Scripts.Data.Localization;
using Scripts.Data.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.MagicBookScene
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

        public async void Init(CombinedMagicData data)
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
            this.data = data;
            magicImage.sprite = data.GetSprite();
            data.magicName = await LocaleUtils.GetStringAsync("Magic",StringUtils.ToCamelCase(data.magicName));
        }
    }
}

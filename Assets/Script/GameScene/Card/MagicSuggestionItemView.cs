using Script.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GameScene
{
    public class MagicSuggestionItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;

        private CombinedMagicData _data;
        private MagicHelperUI _helper;

        public void Setup(CombinedMagicData data, MagicHelperUI helper)
        {
            _data = data;
            _helper = helper;

            if (nameText != null)
                nameText.text = data.magicName; 

            if (iconImage != null)
                iconImage.sprite = data.GetSprite();

            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _helper.OnSuggestionClicked(_data);
        }
    }
}
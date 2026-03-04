using Data;
using Data.Magic;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Card
{
    public class MagicSuggestionItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;

        private CombinedMagicData _data;
        private MagicHelperUI _helper;

        public void Setup(CombinedMagicData data, MagicHelperUI helper)
        {
            _data = data;
            _helper = helper;

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
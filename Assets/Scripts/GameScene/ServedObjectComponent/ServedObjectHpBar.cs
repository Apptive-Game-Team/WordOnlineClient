using UnityEngine;
using UnityEngine.UI;

namespace GameScene.ServedObjectComponent
{
    public class ServedObjectHpBar : MonoBehaviour
    {
        private const string LeftPlayer = "LeftPlayer";
        private const string RightPlayer = "RightPlayer";
        private static readonly Color LeftHpFillColor = new Color(0.92f, 0.24f, 0.24f, 1f);
        private static readonly Color RightHpFillColor = new Color(0.25f, 0.55f, 0.98f, 1f);
        private static readonly Color NeutralHpFillColor = new Color(0.46f, 0.92f, 0.47f, 1f);
        private static readonly Color DefaultBackgroundColor = new Color(0f, 0f, 0f, 0.45f);

        [SerializeField] private ServedObject servedObject;
        private Slider slider;
        private Image fillImage;
        private Image backgroundImage;
        private bool colorsApplied;
    
        private void Awake()
        {
            slider = GetComponentInChildren<Slider>();
            if (slider == null)
            {
                return;
            }

            if (slider.fillRect != null)
            {
                fillImage = slider.fillRect.GetComponent<Image>();
            }

            backgroundImage = FindBackgroundImage();
        }
    
        void Update()
        {
            if (slider == null)
            {
                return;
            }

            if (servedObject == null)
            {
                servedObject = GetComponentInParent<ServedObject>();
                return;
            }

            if (!colorsApplied)
            {
                ApplyTeamColors();
            }
        
            slider.maxValue = servedObject.maxHp;
            slider.value = servedObject.hp;
        }

        private void ApplyTeamColors()
        {
            if (fillImage != null)
            {
                fillImage.color = GetHpFillColor(servedObject.GetMaster());
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = DefaultBackgroundColor;
            }

            colorsApplied = true;
        }

        private Image FindBackgroundImage()
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image != null && image.name == "Background")
                {
                    return image;
                }
            }

            return null;
        }

        private static Color GetHpFillColor(string master)
        {
            switch (master)
            {
                case LeftPlayer:
                    return LeftHpFillColor;
                case RightPlayer:
                    return RightHpFillColor;
                default:
                    return NeutralHpFillColor;
            }
        }
    }
}

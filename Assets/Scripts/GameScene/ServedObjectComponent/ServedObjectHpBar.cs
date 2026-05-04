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
        private Image objectIndicatorImage;
        private bool colorsApplied;
        private string appliedMaster;
    
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
            objectIndicatorImage = FindImageByName("ObjectIndicator");
        }

        void Start()
        {
            if (servedObject == null)
            {
                servedObject = GetComponentInParent<ServedObject>();
            }

            servedObject.OnGaugeChanged += gauge =>
            {
                if (gauge.category.Equals("HP"))
                {
                    slider.maxValue = gauge.maxValue;
                    slider.value = gauge.value;
                }
            };
        }
    
        void Update()
        {
            if (slider == null)
            {
                return;
            }

            string currentMaster = servedObject.GetMaster();
            if (!colorsApplied || appliedMaster != currentMaster)
            {
                ApplyTeamColors();
            }
        }

        private void ApplyTeamColors()
        {
            if (servedObject == null)
            {
                return;
            }

            string currentMaster = servedObject.GetMaster();
            if (fillImage != null)
            {
                fillImage.color = GetHpFillColor(currentMaster);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = DefaultBackgroundColor;
            }

            ApplyObjectIndicator(currentMaster);
            appliedMaster = currentMaster;
            colorsApplied = true;
        }

        private Image FindBackgroundImage()
        {
            return FindImageByName("Background");
        }

        private Image FindImageByName(string imageName)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image != null && image.name == imageName)
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

        public void SetObjectIndicatorMaster(string master)
        {
            if (objectIndicatorImage == null)
            {
                objectIndicatorImage = FindImageByName("ObjectIndicator");
            }

            ApplyObjectIndicator(master);
        }

        private void ApplyObjectIndicator(string master)
        {
            if (objectIndicatorImage == null)
            {
                return;
            }

            bool hasIndicatorColor = TryGetIndicatorColor(master, out Color indicatorColor);
            objectIndicatorImage.enabled = hasIndicatorColor;
            if (hasIndicatorColor)
            {
                objectIndicatorImage.color = indicatorColor;
            }
        }

        private static bool TryGetIndicatorColor(string master, out Color indicatorColor)
        {
            switch (master)
            {
                case LeftPlayer:
                    indicatorColor = LeftHpFillColor;
                    return true;
                case RightPlayer:
                    indicatorColor = RightHpFillColor;
                    return true;
                default:
                    indicatorColor = Color.clear;
                    return false;
            }
        }
    }
}

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
        private RectTransform rootRectTransform;
        private string appliedMaster;
    
        private void Awake()
        {
            rootRectTransform = transform as RectTransform;
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

            string currentMaster = servedObject.GetMaster();
            if (!colorsApplied || appliedMaster != currentMaster)
            {
                ApplyTeamColors();
            }
        
            slider.maxValue = servedObject.maxHp;
            slider.value = servedObject.hp;
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

            appliedMaster = currentMaster;
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

        public bool TryGetTopWorldPosition(float verticalOffset, out Vector3 position)
        {
            if (rootRectTransform == null)
            {
                rootRectTransform = transform as RectTransform;
            }

            if (rootRectTransform == null)
            {
                position = default;
                return false;
            }

            Vector3[] corners = new Vector3[4];
            rootRectTransform.GetWorldCorners(corners);
            position = (corners[1] + corners[2]) * 0.5f + Vector3.up * verticalOffset;
            return true;
        }
    }
}

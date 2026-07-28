using Global;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameScene.ServedObjectComponent
{
    public class ServedObjectWorldUI : MonoBehaviour
    {
        private const string LeftPlayer = "LeftPlayer";
        private const string RightPlayer = "RightPlayer";
        private const string HpCategory = "HP";
        private const string TtlCategory = "TTL";

        private static readonly Color LeftColor = new Color(0.92f, 0.24f, 0.24f, 1f);
        private static readonly Color RightColor = new Color(0.25f, 0.55f, 0.98f, 1f);
        private static readonly Color NeutralColor = new Color(0.46f, 0.92f, 0.47f, 1f);
        private static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.45f);

        [Header("Sections")]
        [SerializeField] private bool showPlayerName;
        [SerializeField] private bool showHp = true;
        [SerializeField] private bool showTtl;
        [SerializeField] private GameObject playerNameSection;
        [SerializeField] private GameObject hpSection;
        [SerializeField] private GameObject ttlSection;

        [Header("References")]
        [SerializeField] private Canvas worldCanvas;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider ttlSlider;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Image hpBackgroundImage;
        [SerializeField] private Image objectIndicatorImage;

        [Header("Placement")]
        [FormerlySerializedAs("verticalOffset")]
        [SerializeField] private float verticalClearance = 0.25f;

        private ServedObject servedObject;
        private bool subscribed;

        public bool CanvasEnabled => worldCanvas != null && worldCanvas.enabled;
        public bool ShowsPlayerName => showPlayerName;

        public ServedObject Owner
        {
            get
            {
                ResolveServedObject();
                return servedObject;
            }
        }

        private void Awake()
        {
            ResolveReferences();
            ApplySectionVisibility();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Initialize()
        {
            ResolveReferences();
            ApplySectionVisibility();
            Subscribe();
            ApplyExistingGauges();
            ApplyMasterPresentation();
            AlignToFixedCamera();
        }

        public void ConfigureSections(bool playerNameVisible, bool hpVisible, bool ttlVisible)
        {
            showPlayerName = playerNameVisible;
            showHp = hpVisible;
            showTtl = ttlVisible;
            ApplySectionVisibility();
        }

        public void SetCanvasEnabled(bool enabled)
        {
            if (worldCanvas != null)
            {
                worldCanvas.enabled = enabled;
            }
        }

        public void SetObjectIndicatorMaster(string master)
        {
            ApplyObjectIndicator(master);
        }

        private void AlignToFixedCamera()
        {
            Camera worldCamera = Camera.main;
            if (worldCamera == null || servedObject == null)
            {
                return;
            }

            transform.rotation = worldCamera.transform.rotation;

            SpriteRenderer spriteRenderer = ResolveSpriteRenderer();
            Vector3 anchorPosition = servedObject.GetActualTransform().position;
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                anchorPosition = GetScreenTopAnchor(
                    spriteRenderer,
                    servedObject.GetComponentsInChildren<Collider2D>(true),
                    worldCamera.transform.up);
            }

            float uiBottom = GetActiveUiBottom();
            float rootOffset = verticalClearance - uiBottom;
            transform.position = anchorPosition + worldCamera.transform.up * rootOffset;
        }

        private static Vector3 GetScreenTopAnchor(
            SpriteRenderer spriteRenderer,
            Collider2D[] colliders,
            Vector3 screenUp)
        {
            Bounds spriteBounds = spriteRenderer.sprite.bounds;
            Vector3 worldCenter = spriteRenderer.transform.TransformPoint(spriteBounds.center);
            float topProjection = Vector3.Dot(worldCenter, screenUp);

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Vector3 localCorner = new Vector3(
                        x == 0 ? spriteBounds.min.x : spriteBounds.max.x,
                        y == 0 ? spriteBounds.min.y : spriteBounds.max.y,
                        spriteBounds.center.z);
                    Vector3 worldCorner = spriteRenderer.transform.TransformPoint(localCorner);
                    topProjection = Mathf.Max(topProjection, Vector3.Dot(worldCorner, screenUp));
                }
            }

            foreach (Collider2D collider in colliders)
            {
                if (collider == null || !collider.enabled)
                {
                    continue;
                }

                Bounds colliderBounds = collider.bounds;
                Vector3 center = colliderBounds.center;
                Vector3 extents = colliderBounds.extents;
                float projectedExtent =
                    Mathf.Abs(screenUp.x) * extents.x +
                    Mathf.Abs(screenUp.y) * extents.y +
                    Mathf.Abs(screenUp.z) * extents.z;
                topProjection = Mathf.Max(
                    topProjection,
                    Vector3.Dot(center, screenUp) + projectedExtent);
            }

            float centerProjection = Vector3.Dot(worldCenter, screenUp);
            return worldCenter + screenUp * (topProjection - centerProjection);
        }

        private float GetActiveUiBottom()
        {
            float bottom = float.PositiveInfinity;
            IncludeSectionBottom(playerNameSection, showPlayerName, ref bottom);
            IncludeSectionBottom(hpSection, showHp, ref bottom);
            IncludeSectionBottom(ttlSection, showTtl, ref bottom);
            return float.IsPositiveInfinity(bottom) ? 0f : bottom;
        }

        private void IncludeSectionBottom(GameObject section, bool visible, ref float bottom)
        {
            if (!visible || section == null)
            {
                return;
            }

            RectTransform sectionRect = section.transform as RectTransform;
            if (sectionRect == null)
            {
                return;
            }

            Bounds sectionBounds =
                RectTransformUtility.CalculateRelativeRectTransformBounds(transform, sectionRect);
            bottom = Mathf.Min(bottom, sectionBounds.min.y);
        }

        private SpriteRenderer ResolveSpriteRenderer()
        {
            Transform actualTransform = servedObject.GetActualTransform();
            if (actualTransform != null)
            {
                SpriteRenderer actualRenderer = actualTransform.GetComponent<SpriteRenderer>();
                if (actualRenderer != null)
                {
                    return actualRenderer;
                }

                actualRenderer = actualTransform.GetComponentInChildren<SpriteRenderer>();
                if (actualRenderer != null)
                {
                    return actualRenderer;
                }
            }

            return servedObject.GetComponentInChildren<SpriteRenderer>();
        }

        private void Subscribe()
        {
            if (subscribed || !ResolveServedObject())
            {
                return;
            }

            servedObject.OnGaugeChanged += OnGaugeUpdated;
            servedObject.OnMasterChanged += ApplyMasterPresentation;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || servedObject == null)
            {
                return;
            }

            servedObject.OnGaugeChanged -= OnGaugeUpdated;
            servedObject.OnMasterChanged -= ApplyMasterPresentation;
            subscribed = false;
        }

        private bool ResolveServedObject()
        {
            if (servedObject != null)
            {
                return true;
            }

            servedObject = GetComponentInParent<ServedObject>();
            return servedObject != null;
        }

        private void ResolveReferences()
        {
            if (worldCanvas == null)
            {
                worldCanvas = GetComponentInChildren<Canvas>(true);
            }

            if (playerNameText == null)
            {
                playerNameText = FindComponentByName<TextMeshProUGUI>("PlayerName");
            }

            if (hpSlider == null)
            {
                hpSlider = FindComponentByName<Slider>("HpBar");
            }

            if (ttlSlider == null)
            {
                ttlSlider = FindComponentByName<Slider>("TtlBar");
            }

            playerNameSection = playerNameSection != null
                ? playerNameSection
                : playerNameText != null ? playerNameText.gameObject : null;
            hpSection = hpSection != null ? hpSection : hpSlider != null ? hpSlider.gameObject : null;
            ttlSection = ttlSection != null ? ttlSection : ttlSlider != null ? ttlSlider.gameObject : null;

            if (hpSlider != null)
            {
                hpFillImage = hpFillImage != null
                    ? hpFillImage
                    : hpSlider.fillRect != null ? hpSlider.fillRect.GetComponent<Image>() : null;
            }

            hpBackgroundImage = hpBackgroundImage != null
                ? hpBackgroundImage
                : FindImageByName(hpSection, "Background");
            objectIndicatorImage = objectIndicatorImage != null
                ? objectIndicatorImage
                : FindComponentByName<Image>("ObjectIndicator");
        }

        private T FindComponentByName<T>(string objectName) where T : Component
        {
            T[] components = GetComponentsInChildren<T>(true);
            foreach (T component in components)
            {
                if (component.name == objectName)
                {
                    return component;
                }
            }

            return null;
        }

        private static Image FindImageByName(GameObject root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            Image[] images = root.GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image.name == objectName)
                {
                    return image;
                }
            }

            return null;
        }

        private void ApplySectionVisibility()
        {
            if (playerNameSection != null)
            {
                playerNameSection.SetActive(showPlayerName);
            }

            if (hpSection != null)
            {
                hpSection.SetActive(showHp);
            }

            if (ttlSection != null)
            {
                ttlSection.SetActive(showTtl);
            }
        }

        private void OnGaugeUpdated(Gauge gauge)
        {
            if (gauge == null)
            {
                return;
            }

            if (showHp && hpSlider != null && gauge.category == HpCategory)
            {
                ApplyGauge(hpSlider, gauge);
            }
            else if (showTtl && ttlSlider != null && gauge.category == TtlCategory)
            {
                ApplyGauge(ttlSlider, gauge);
            }
        }

        private void ApplyExistingGauges()
        {
            if (servedObject == null)
            {
                return;
            }

            foreach (Gauge gauge in servedObject.gauges)
            {
                OnGaugeUpdated(gauge);
            }
        }

        private static void ApplyGauge(Slider slider, Gauge gauge)
        {
            slider.maxValue = gauge.maxValue;
            slider.value = gauge.value;
        }

        private void ApplyMasterPresentation()
        {
            if (!ResolveServedObject())
            {
                return;
            }

            string master = servedObject.GetMaster();
            Color teamColor = GetTeamColor(master);
            if (hpFillImage != null)
            {
                hpFillImage.color = teamColor;
            }

            if (hpBackgroundImage != null)
            {
                hpBackgroundImage.color = BackgroundColor;
            }

            ApplyObjectIndicator(master);
            ApplyPlayerName(master);
        }

        private void ApplyPlayerName(string master)
        {
            if (!showPlayerName || playerNameText == null)
            {
                return;
            }

            if (SceneContext.MatchInfo == null)
            {
                playerNameText.text = string.Empty;
                return;
            }

            switch (master)
            {
                case LeftPlayer:
                    playerNameText.text = SceneContext.MatchInfo.leftUser.name;
                    break;
                case RightPlayer:
                    playerNameText.text = SceneContext.MatchInfo.rightUser.name;
                    break;
                default:
                    playerNameText.text = string.Empty;
                    break;
            }
        }

        private void ApplyObjectIndicator(string master)
        {
            if (objectIndicatorImage == null)
            {
                return;
            }

            bool hasTeam = master == LeftPlayer || master == RightPlayer;
            objectIndicatorImage.enabled = hasTeam && showHp;
            if (hasTeam)
            {
                objectIndicatorImage.color = GetTeamColor(master);
            }
        }

        private static Color GetTeamColor(string master)
        {
            switch (master)
            {
                case LeftPlayer:
                    return LeftColor;
                case RightPlayer:
                    return RightColor;
                default:
                    return NeutralColor;
            }
        }
    }
}

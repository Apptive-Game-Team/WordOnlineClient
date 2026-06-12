using GameScene.Card;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameScene.ServedObjectComponent
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Selectable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private const float GroundZ = 0f;
        private const string HoverIndicatorResourcePath = "UI/ObjectIndicator";
        private static Sprite hoverIndicatorSprite;

        [SerializeField] private CardInputSender cardInputSender;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float hoverIndicatorScale = 0.3f;

        private bool isHovered;
        private GameObject hoverTargetIndicator;
        private GameObject hoverGroundIndicator;
        private SpriteRenderer hoverTargetRenderer;
        private SpriteRenderer hoverGroundRenderer;

        void Awake()
        {
            if (!worldCamera) worldCamera = Camera.main;
            if (!cardInputSender) cardInputSender = FindObjectOfType<CardInputSender>();
        
            var box = GetComponent<BoxCollider2D>();
            var sr  = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            if (sr && sr.sprite)
            {
                var b = sr.sprite.bounds;
                box.size   = b.size;
                box.offset = b.center;
                box.isTrigger = true;
            }
        
            if (worldCamera && !worldCamera.GetComponent<Physics2DRaycaster>())
                worldCamera.gameObject.AddComponent<Physics2DRaycaster>();
        }

        private void LateUpdate()
        {
            if (isHovered)
            {
                UpdateHoverIndicators();
            }
        }

        private void OnDisable()
        {
            HideHoverIndicators();
        }

        private void OnDestroy()
        {
            if (hoverTargetIndicator != null)
            {
                Destroy(hoverTargetIndicator);
            }

            if (hoverGroundIndicator != null)
            {
                Destroy(hoverGroundIndicator);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            EnsureHoverIndicators();
            UpdateHoverIndicators();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            HideHoverIndicators();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!cardInputSender) return;
            var pos = transform.parent.position;
        
            cardInputSender.SendInput(pos);
        }

        private void EnsureHoverIndicators()
        {
            if (hoverTargetIndicator == null)
            {
                hoverTargetIndicator = CreateHoverIndicator("HoveredObjectIndicator", new Color(1f, 1f, 1f, 0.85f), out hoverTargetRenderer);
            }

            if (hoverGroundIndicator == null)
            {
                hoverGroundIndicator = CreateHoverIndicator("HoveredGroundIndicator", new Color(1f, 1f, 1f, 0.55f), out hoverGroundRenderer);
            }
        }

        private GameObject CreateHoverIndicator(string indicatorName, Color color, out SpriteRenderer spriteRenderer)
        {
            GameObject indicatorObject = new GameObject(indicatorName);
            indicatorObject.transform.localScale = Vector3.one * hoverIndicatorScale;

            spriteRenderer = indicatorObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetHoverIndicatorSprite();
            spriteRenderer.color = color;
            ApplyIndicatorSorting(spriteRenderer, 20);

            indicatorObject.SetActive(false);
            return indicatorObject;
        }

        private void UpdateHoverIndicators()
        {
            EnsureHoverIndicators();

            Vector3 targetPosition = GetTargetWorldPosition();
            Vector3 groundPosition = targetPosition;
            groundPosition.z = GroundZ;

            hoverTargetIndicator.transform.position = targetPosition;
            hoverTargetIndicator.SetActive(true);

            bool shouldShowGroundIndicator = Mathf.Abs(targetPosition.z - GroundZ) > Mathf.Epsilon;
            hoverGroundIndicator.SetActive(shouldShowGroundIndicator);
            if (shouldShowGroundIndicator)
            {
                hoverGroundIndicator.transform.position = groundPosition;
            }

            ApplyIndicatorSorting(hoverTargetRenderer, 20);
            ApplyIndicatorSorting(hoverGroundRenderer, 21);
        }

        private Vector3 GetTargetWorldPosition()
        {
            ServedObject servedObject = GetComponentInParent<ServedObject>();
            if (servedObject != null)
            {
                return servedObject.GetActualTransform().position;
            }

            return transform.parent != null ? transform.parent.position : transform.position;
        }

        private void HideHoverIndicators()
        {
            if (hoverTargetIndicator != null)
            {
                hoverTargetIndicator.SetActive(false);
            }

            if (hoverGroundIndicator != null)
            {
                hoverGroundIndicator.SetActive(false);
            }
        }

        private void ApplyIndicatorSorting(SpriteRenderer indicatorRenderer, int orderOffset)
        {
            if (indicatorRenderer == null)
            {
                return;
            }

            SpriteRenderer targetRenderer =
                GetComponent<SpriteRenderer>() ??
                GetComponentInChildren<SpriteRenderer>() ??
                GetComponentInParent<SpriteRenderer>();

            if (targetRenderer == null)
            {
                indicatorRenderer.sortingOrder = orderOffset;
                return;
            }

            indicatorRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            indicatorRenderer.sortingOrder = targetRenderer.sortingOrder + orderOffset;
        }

        private static Sprite GetHoverIndicatorSprite()
        {
            if (hoverIndicatorSprite != null)
            {
                return hoverIndicatorSprite;
            }

            hoverIndicatorSprite = Resources.Load<Sprite>(HoverIndicatorResourcePath);
            return hoverIndicatorSprite;
        }
    }
}

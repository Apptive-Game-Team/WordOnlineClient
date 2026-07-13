using GameScene.Card;
using Global;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameScene.ServedObjectComponent
{
    public class Selectable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private const string HitboxObjectName = "SelectableHitbox";

        [SerializeField] private Camera worldCamera;
        [SerializeField] private Behaviour outlineComponent;
        [SerializeField, Min(0.1f)] private float hitboxDepth = 0.5f;

        private SpriteRenderer _spriteRenderer;
        private BoxCollider _selectionCollider;
        private Bounds _lastRendererBounds;
        private bool _hasRendererBounds;
        private bool _isPointerOver;

        void Awake()
        {
            if (!worldCamera) worldCamera = Camera.main;

            _spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            outlineComponent ??= FindOutlineComponent();
            if (outlineComponent != null)
                outlineComponent.enabled = false;

            if (_spriteRenderer == null)
            {
                WDebug.LogWarning($"{nameof(Selectable)} could not find a SpriteRenderer for {name}.");
                return;
            }

            _selectionCollider = GetOrCreateSelectionCollider(_spriteRenderer);
            SyncSelectionCollider(true);

            if (worldCamera && !worldCamera.GetComponent<PhysicsRaycaster>())
                worldCamera.gameObject.AddComponent<PhysicsRaycaster>();
        }

        private void LateUpdate()
        {
            SyncSelectionCollider(false);

            if (_isPointerOver)
            {
                SelectionGroundIndicator.Show(transform, GetSelectionPosition());
            }
        }

        private void OnDisable()
        {
            _isPointerOver = false;
            SelectionGroundIndicator.Hide(transform);
            if (outlineComponent != null)
            {
                outlineComponent.enabled = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

            _isPointerOver = true;
            SelectionGroundIndicator.Show(transform, GetSelectionPosition());
            if (outlineComponent != null)
            {
                outlineComponent.enabled = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOver = false;
            SelectionGroundIndicator.Hide(transform);
            if (outlineComponent != null)
            {
                outlineComponent.enabled = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _isPointerOver = false;
            SelectionGroundIndicator.Hide(transform);
            if (outlineComponent != null)
            {
                outlineComponent.enabled = false;
            }

            Vector3 pos = GetSelectionPosition();
            pos.y = 0f; // 공중 유닛도 서버가 기대하는 ground-level 좌표로 전송

            CardInputSender.Instance.SendInput(pos);
        }

        private Vector3 GetSelectionPosition()
        {
            Transform target = transform.parent != null ? transform.parent : transform;
            return target.position;
        }

        private BoxCollider GetOrCreateSelectionCollider(SpriteRenderer spriteRenderer)
        {
            Transform hitboxTransform = spriteRenderer.transform.Find(HitboxObjectName);
            if (hitboxTransform == null)
            {
                GameObject hitboxObject = new GameObject(HitboxObjectName);
                hitboxTransform = hitboxObject.transform;
                hitboxTransform.SetParent(spriteRenderer.transform, false);
            }

            hitboxTransform.gameObject.layer = spriteRenderer.gameObject.layer;
            BoxCollider boxCollider = hitboxTransform.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = hitboxTransform.gameObject.AddComponent<BoxCollider>();
            }

            boxCollider.isTrigger = true;
            return boxCollider;
        }

        private void SyncSelectionCollider(bool force)
        {
            if (_spriteRenderer == null || _selectionCollider == null)
            {
                return;
            }

            Bounds rendererBounds = _spriteRenderer.localBounds;
            _selectionCollider.enabled = _spriteRenderer.enabled && _spriteRenderer.sprite != null;
            if (!force && _hasRendererBounds && BoundsApproximatelyEqual(rendererBounds, _lastRendererBounds))
            {
                return;
            }

            float safeDepth = Mathf.Max(hitboxDepth, 0.1f);
            _selectionCollider.center = rendererBounds.center;
            _selectionCollider.size = new Vector3(rendererBounds.size.x, rendererBounds.size.y, safeDepth);
            _lastRendererBounds = rendererBounds;
            _hasRendererBounds = true;
        }

        private static bool BoundsApproximatelyEqual(Bounds left, Bounds right)
        {
            return (left.center - right.center).sqrMagnitude <= Mathf.Epsilon &&
                   (left.size - right.size).sqrMagnitude <= Mathf.Epsilon;
        }

        private Behaviour FindOutlineComponent()
        {
            Behaviour[] candidates = GetComponentsInChildren<Behaviour>(true);
            foreach (Behaviour candidate in candidates)
            {
                if (candidate == null || candidate == this) continue;
                if (candidate.GetType().Name.Contains("Outlin"))
                    return candidate;
            }

            return null;
        }
    }
}

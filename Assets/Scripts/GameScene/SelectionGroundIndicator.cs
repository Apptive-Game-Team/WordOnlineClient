using UnityEngine;

namespace GameScene
{
    public class SelectionGroundIndicator : MonoBehaviour
    {
        private const string IndicatorObjectName = "SelectionGroundIndicator";
        private const int SortingOrder = 17;
        private const float LineWidth = 0.05f;
        private const float GroundY = 0f;

        private static readonly Color LineColor = new Color(0.18f, 0.88f, 0.82f, 0.9f);
        private static SelectionGroundIndicator instance;

        private LineRenderer lineRenderer;
        private Transform owner;

        public static void Show(Transform indicatorOwner, Vector3 selectedPosition)
        {
            if (indicatorOwner == null || Mathf.Approximately(selectedPosition.y, GroundY))
            {
                Hide(indicatorOwner);
                return;
            }

            EnsureInstance();
            instance.owner = indicatorOwner;
            instance.SetPositions(selectedPosition, ProjectToGround(selectedPosition));
            instance.lineRenderer.enabled = true;
        }

        public static void Hide(Transform indicatorOwner)
        {
            if (instance == null || (instance.owner != null && instance.owner != indicatorOwner))
            {
                return;
            }

            instance.owner = null;
            instance.lineRenderer.enabled = false;
        }

        public static Vector3 ProjectToGround(Vector3 position)
        {
            position.y = GroundY;
            return position;
        }

        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            GameObject indicatorObject = new GameObject(IndicatorObjectName);
            instance = indicatorObject.AddComponent<SelectionGroundIndicator>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = LineWidth;
            lineRenderer.endWidth = LineWidth;
            lineRenderer.startColor = LineColor;
            lineRenderer.endColor = LineColor;
            lineRenderer.numCapVertices = 4;
            lineRenderer.sortingOrder = SortingOrder;
            lineRenderer.enabled = false;
        }

        private void LateUpdate()
        {
            if (owner == null && lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (lineRenderer != null && lineRenderer.sharedMaterial != null)
            {
                Destroy(lineRenderer.sharedMaterial);
            }

            if (instance == this)
            {
                instance = null;
            }
        }

        private void SetPositions(Vector3 selectedPosition, Vector3 groundPosition)
        {
            lineRenderer.SetPosition(0, selectedPosition);
            lineRenderer.SetPosition(1, groundPosition);
        }
    }
}

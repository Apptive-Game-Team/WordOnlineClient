using UnityEngine;

namespace GameScene
{
    public class SkillIndicatorShapeRenderer : MonoBehaviour
    {
        private const int CircleSegments = 64;
        private const float DefaultZOffset = -0.02f;

        private static readonly Color DefaultFillColor = new Color(0.95f, 0.62f, 0.18f, 0.24f);
        private static readonly Color DefaultEdgeColor = new Color(0.18f, 0.88f, 0.82f, 0.62f);
        private static Material sharedMaterial;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private LineRenderer edgeRenderer;
        private SpriteRenderer spriteRenderer;
        private Transform fillTransform;
        private Transform edgeTransform;
        private Mesh shapeMesh;
        private Vector3[] circleVertices;
        private int[] circleTriangles;
        private Color[] circleColors;
        private readonly Vector3[] lineVertices = new Vector3[4];
        private readonly int[] lineTriangles = { 0, 2, 1, 0, 3, 2 };
        private readonly Color[] lineColors =
        {
            DefaultFillColor,
            DefaultFillColor,
            DefaultFillColor,
            DefaultFillColor
        };

        private void Awake()
        {
            EnsureRenderers();
            DisableSpriteRenderer();
        }

        private void OnDestroy()
        {
            if (shapeMesh != null)
            {
                Destroy(shapeMesh);
                shapeMesh = null;
            }
        }

        public void SetCircle(Vector3 position, float radius, bool includeFill, int sortingOrder, float edgeWidth)
        {
            EnsureRenderers();
            DisableSpriteRenderer();

            transform.position = new Vector3(position.x, position.y, DefaultZOffset);
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            float safeRadius = Mathf.Max(radius, 0f);
            if (safeRadius <= Mathf.Epsilon)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            SetSorting(sortingOrder);
            BuildCircleMesh(safeRadius, includeFill);
            if (edgeWidth > 0f)
            {
                BuildCircleEdge(safeRadius, edgeWidth);
            }
            else
            {
                edgeRenderer.enabled = false;
            }
        }

        public void SetLocalCircle(float radius, int sortingOrder)
        {
            EnsureRenderers();
            DisableSpriteRenderer();

            transform.localScale = Vector3.one;

            float safeRadius = Mathf.Max(radius, 0f);
            if (safeRadius <= Mathf.Epsilon)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            SetSorting(sortingOrder);
            BuildCircleMesh(safeRadius, true);
            edgeRenderer.enabled = false;
        }

        public void SetLine(Vector3 startPosition, Vector3 targetPosition, float length, int sortingOrder, float width)
        {
            EnsureRenderers();
            DisableSpriteRenderer();

            float safeLength = Mathf.Max(length, 0f);
            if (safeLength <= Mathf.Epsilon)
            {
                SetVisible(false);
                return;
            }

            Vector3 direction = targetPosition - startPosition;
            direction.z = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            SetSorting(sortingOrder);

            transform.position = new Vector3(startPosition.x, startPosition.y, DefaultZOffset);
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            transform.localScale = Vector3.one;

            float halfWidth = Mathf.Max(width, 0.01f) * 0.5f;
            BuildLineMesh(safeLength, halfWidth);
            edgeRenderer.enabled = false;
        }

        private void EnsureRenderers()
        {
            if (meshFilter == null)
            {
                fillTransform = GetOrCreateChild("ShapeFill");
                meshFilter = fillTransform.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    meshFilter = fillTransform.gameObject.AddComponent<MeshFilter>();
                }
            }

            if (meshRenderer == null)
            {
                if (fillTransform == null)
                {
                    fillTransform = GetOrCreateChild("ShapeFill");
                }

                meshRenderer = fillTransform.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    meshRenderer = fillTransform.gameObject.AddComponent<MeshRenderer>();
                }
                meshRenderer.sharedMaterial = GetSharedMaterial();
            }

            if (edgeRenderer == null)
            {
                edgeTransform = GetOrCreateChild("ShapeEdge");
                edgeRenderer = edgeTransform.GetComponent<LineRenderer>();
                if (edgeRenderer == null)
                {
                    edgeRenderer = edgeTransform.gameObject.AddComponent<LineRenderer>();
                }

                edgeRenderer.sharedMaterial = GetSharedMaterial();
                edgeRenderer.useWorldSpace = false;
                edgeRenderer.loop = true;
                edgeRenderer.numCornerVertices = 4;
                edgeRenderer.numCapVertices = 4;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private Transform GetOrCreateChild(string childName)
        {
            Transform child = transform.Find(childName);
            if (child == null)
            {
                child = new GameObject(childName).transform;
                child.SetParent(transform, false);
            }

            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
            return child;
        }

        private void DisableSpriteRenderer()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }

        private void SetVisible(bool visible)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = visible;
            }

            if (edgeRenderer != null)
            {
                edgeRenderer.enabled = visible;
            }
        }

        private void SetSorting(int sortingOrder)
        {
            meshRenderer.sortingOrder = sortingOrder;
            edgeRenderer.sortingOrder = sortingOrder + 1;
        }

        private void BuildCircleMesh(float radius, bool includeFill)
        {
            if (!includeFill)
            {
                meshFilter.sharedMesh = null;
                return;
            }

            EnsureCircleArrays();

            circleVertices[0] = Vector3.zero;
            for (int i = 0; i < CircleSegments; i++)
            {
                float angle = Mathf.PI * 2f * i / CircleSegments;
                circleVertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            }

            Mesh mesh = GetShapeMesh();
            mesh.Clear();
            mesh.vertices = circleVertices;
            mesh.triangles = circleTriangles;
            mesh.colors = circleColors;
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }

        private void BuildCircleEdge(float radius, float edgeWidth)
        {
            edgeRenderer.loop = true;
            edgeRenderer.positionCount = CircleSegments;
            edgeRenderer.startWidth = edgeWidth;
            edgeRenderer.endWidth = edgeWidth;
            edgeRenderer.startColor = DefaultEdgeColor;
            edgeRenderer.endColor = DefaultEdgeColor;

            for (int i = 0; i < CircleSegments; i++)
            {
                float angle = Mathf.PI * 2f * i / CircleSegments;
                edgeRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }
        }

        private void BuildLineMesh(float length, float halfWidth)
        {
            lineVertices[0] = new Vector3(0f, -halfWidth, 0f);
            lineVertices[1] = new Vector3(length, -halfWidth, 0f);
            lineVertices[2] = new Vector3(length, halfWidth, 0f);
            lineVertices[3] = new Vector3(0f, halfWidth, 0f);

            Mesh mesh = GetShapeMesh();
            mesh.Clear();
            mesh.vertices = lineVertices;
            mesh.triangles = lineTriangles;
            mesh.colors = lineColors;
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }

        private void BuildLineEdge(float length, float halfWidth)
        {
            edgeRenderer.loop = true;
            edgeRenderer.positionCount = 4;
            edgeRenderer.startWidth = 0.035f;
            edgeRenderer.endWidth = 0.035f;
            edgeRenderer.startColor = DefaultEdgeColor;
            edgeRenderer.endColor = DefaultEdgeColor;
            edgeRenderer.SetPosition(0, new Vector3(0f, -halfWidth, 0f));
            edgeRenderer.SetPosition(1, new Vector3(length, -halfWidth, 0f));
            edgeRenderer.SetPosition(2, new Vector3(length, halfWidth, 0f));
            edgeRenderer.SetPosition(3, new Vector3(0f, halfWidth, 0f));
        }

        private Mesh GetShapeMesh()
        {
            if (shapeMesh == null)
            {
                shapeMesh = new Mesh { name = "SkillIndicatorShape" };
            }

            return shapeMesh;
        }

        private void EnsureCircleArrays()
        {
            if (circleVertices != null)
            {
                return;
            }

            circleVertices = new Vector3[CircleSegments + 1];
            circleTriangles = new int[CircleSegments * 3];
            circleColors = new Color[CircleSegments + 1];

            for (int i = 0; i < CircleSegments; i++)
            {
                int triangleIndex = i * 3;
                circleTriangles[triangleIndex] = 0;
                circleTriangles[triangleIndex + 1] = i + 1;
                circleTriangles[triangleIndex + 2] = i == CircleSegments - 1 ? 1 : i + 2;
            }

            for (int i = 0; i < circleColors.Length; i++)
            {
                circleColors[i] = DefaultFillColor;
            }
        }

        private static Material GetSharedMaterial()
        {
            if (sharedMaterial != null)
            {
                return sharedMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            sharedMaterial = new Material(shader);
            return sharedMaterial;
        }
    }
}

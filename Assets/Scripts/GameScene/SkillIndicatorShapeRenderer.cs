using System.Collections.Generic;
using UnityEngine;

namespace GameScene
{
    public class SkillIndicatorShapeRenderer : MonoBehaviour
    {
        private const int CircleSegments = 64;
        private const float DefaultSurfaceOffset = 0.02f;

        // 바닥 패널 월드 범위: X ∈ [0, 18], Z ∈ [0, 10]
        private static readonly Vector2 FieldMin = new Vector2(0f, 0f);
        private static readonly Vector2 FieldMax = new Vector2(18f, 10f);

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

            transform.position = GetSurfacePosition(position);
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
            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            SetSorting(sortingOrder);

            transform.position = GetSurfacePosition(startPosition);
            transform.rotation = Quaternion.Euler(0f, -Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg, 0f);
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

        // ─── Circle ──────────────────────────────────────────────────────────────

        private void BuildCircleMesh(float radius, bool includeFill)
        {
            if (!includeFill)
            {
                meshFilter.sharedMesh = null;
                return;
            }

            // 원의 외곽 polygon을 로컬 XZ 좌표로 생성
            var polygon = new List<Vector2>(CircleSegments);
            for (int i = 0; i < CircleSegments; i++)
            {
                float angle = Mathf.PI * 2f * i / CircleSegments;
                polygon.Add(new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }

            // 월드 XZ 공간에서 필드 경계로 클리핑 후 로컬 XZ로 역변환
            List<Vector2> clipped = ClipPolygonToFieldInWorldSpace(polygon);

            if (clipped == null || clipped.Count < 3)
            {
                meshFilter.sharedMesh = null;
                return;
            }

            // Fan triangulation
            int vertCount = clipped.Count;
            var verts = new Vector3[vertCount];
            var colors = new Color[vertCount];
            var tris = new int[(vertCount - 2) * 3];

            for (int i = 0; i < vertCount; i++)
            {
                verts[i] = new Vector3(clipped[i].x, 0f, clipped[i].y);
                colors[i] = DefaultFillColor;
            }

            for (int i = 0; i < vertCount - 2; i++)
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 2;
                tris[i * 3 + 2] = i + 1;
            }

            Mesh mesh = GetShapeMesh();
            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.colors = colors;
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
                edgeRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        // ─── Line ────────────────────────────────────────────────────────────────

        // 반원 캡 세그먼트 수 (클수록 부드럽고, 삼각형 수 증가)
        private const int CapSegments = 16;

        private void BuildLineMesh(float length, float halfWidth)
        {
            // Stadium(rounded rectangle) polygon:
            //  - 우측 반원 캡 (center: length, 0),  각도 -90° → +90° (CCW)
            //  - 좌측 반원 캡 (center: 0,      0),  각도 +90° → +270° (CCW)
            int totalVerts = CapSegments * 2 + 2; // 각 반원 CapSegments+1개 점, 연결 2개
            var polygon = new List<Vector2>(totalVerts);

            // 우측 캡: X = length, 반시계 방향 (아래 → 위)
            for (int i = 0; i <= CapSegments; i++)
            {
                float angle = Mathf.PI * (-0.5f + (float)i / CapSegments); // -90° → +90°
                polygon.Add(new Vector2(length + Mathf.Cos(angle) * halfWidth,
                                                Mathf.Sin(angle) * halfWidth));
            }

            // 좌측 캡: X = 0, 반시계 방향 (위 → 아래)
            for (int i = 0; i <= CapSegments; i++)
            {
                float angle = Mathf.PI * (0.5f + (float)i / CapSegments); // 90° → 270°
                polygon.Add(new Vector2(0f + Mathf.Cos(angle) * halfWidth,
                                             Mathf.Sin(angle) * halfWidth));
            }

            // 월드 XZ 공간에서 필드 경계로 클리핑 후 로컬 XZ로 역변환
            List<Vector2> clipped = ClipPolygonToFieldInWorldSpace(polygon);

            if (clipped == null || clipped.Count < 3)
            {
                meshFilter.sharedMesh = null;
                return;
            }

            int vertCount = clipped.Count;
            var verts = new Vector3[vertCount];
            var colors = new Color[vertCount];
            var tris = new int[(vertCount - 2) * 3];

            for (int i = 0; i < vertCount; i++)
            {
                verts[i] = new Vector3(clipped[i].x, 0f, clipped[i].y);
                colors[i] = DefaultFillColor;
            }

            for (int i = 0; i < vertCount - 2; i++)
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 2;
                tris[i * 3 + 2] = i + 1;
            }

            Mesh mesh = GetShapeMesh();
            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.colors = colors;
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
            edgeRenderer.SetPosition(0, new Vector3(0f, 0f, -halfWidth));
            edgeRenderer.SetPosition(1, new Vector3(length, 0f, -halfWidth));
            edgeRenderer.SetPosition(2, new Vector3(length, 0f, halfWidth));
            edgeRenderer.SetPosition(3, new Vector3(0f, 0f, halfWidth));
        }

        // ─── Clipping helpers ────────────────────────────────────────────────────

        /// <summary>
        /// 로컬 XZ polygon을 월드 XZ로 변환 → 필드 AABB로 클리핑 → 다시 로컬 XZ로 역변환합니다.
        /// rotation이 있는 transform(예: LineSkillIndicator)에서도 정확히 동작합니다.
        /// </summary>
        private List<Vector2> ClipPolygonToFieldInWorldSpace(List<Vector2> localPolygon)
        {
            // 1) 로컬 XZ → 월드 XZ
            var worldPolygon = new List<Vector2>(localPolygon.Count);
            foreach (Vector2 local in localPolygon)
            {
                Vector3 worldPos = transform.TransformPoint(new Vector3(local.x, 0f, local.y));
                worldPolygon.Add(new Vector2(worldPos.x, worldPos.z));
            }

            // 2) 월드 XZ AABB 클리핑
            List<Vector2> clippedWorld = ClipPolygonToAABB(worldPolygon, FieldMin, FieldMax);
            if (clippedWorld == null || clippedWorld.Count < 3) return null;

            // 3) 월드 XZ → 로컬 XZ
            float worldY = transform.position.y;
            var result = new List<Vector2>(clippedWorld.Count);
            foreach (Vector2 world in clippedWorld)
            {
                Vector3 localPos = transform.InverseTransformPoint(new Vector3(world.x, worldY, world.y));
                result.Add(new Vector2(localPos.x, localPos.z));
            }
            return result;
        }

        /// <summary>
        /// Sutherland-Hodgman algorithm: AABB(minX,minZ)~(maxX,maxZ)로 XZ polygon을 클리핑합니다.
        /// </summary>
        private static List<Vector2> ClipPolygonToAABB(List<Vector2> polygon, Vector2 min, Vector2 max)
        {
            if (polygon == null || polygon.Count < 3) return null;

            // 4개의 clip edge: left, right, bottom, top (XZ 평면)
            List<Vector2> output = polygon;

            // Left  (X >= min.x)
            output = ClipByEdge(output, p => p.x >= min.x, (a, b) =>
            {
                float t = (min.x - a.x) / (b.x - a.x);
                return new Vector2(min.x, a.y + t * (b.y - a.y));
            });
            if (output == null || output.Count < 3) return null;

            // Right (X <= max.x)
            output = ClipByEdge(output, p => p.x <= max.x, (a, b) =>
            {
                float t = (max.x - a.x) / (b.x - a.x);
                return new Vector2(max.x, a.y + t * (b.y - a.y));
            });
            if (output == null || output.Count < 3) return null;

            // Bottom (Y >= min.y)
            output = ClipByEdge(output, p => p.y >= min.y, (a, b) =>
            {
                float t = (min.y - a.y) / (b.y - a.y);
                return new Vector2(a.x + t * (b.x - a.x), min.y);
            });
            if (output == null || output.Count < 3) return null;

            // Top (Y <= max.y)
            output = ClipByEdge(output, p => p.y <= max.y, (a, b) =>
            {
                float t = (max.y - a.y) / (b.y - a.y);
                return new Vector2(a.x + t * (b.x - a.x), max.y);
            });

            return output != null && output.Count >= 3 ? output : null;
        }

        private delegate bool InsideTest(Vector2 p);
        private delegate Vector2 IntersectCalc(Vector2 a, Vector2 b);

        private static List<Vector2> ClipByEdge(
            List<Vector2> input,
            InsideTest inside,
            IntersectCalc intersect)
        {
            if (input == null || input.Count == 0) return null;

            var output = new List<Vector2>(input.Count);
            int count = input.Count;

            for (int i = 0; i < count; i++)
            {
                Vector2 current = input[i];
                Vector2 previous = input[(i + count - 1) % count];

                bool curInside = inside(current);
                bool prevInside = inside(previous);

                if (curInside)
                {
                    if (!prevInside)
                    {
                        output.Add(intersect(previous, current));
                    }
                    output.Add(current);
                }
                else if (prevInside)
                {
                    output.Add(intersect(previous, current));
                }
            }

            return output.Count >= 3 ? output : null;
        }

        // ─── Shared helpers ──────────────────────────────────────────────────────

        private static Vector3 GetSurfacePosition(Vector3 position)
        {
            position.y += DefaultSurfaceOffset;
            return position;
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
                circleTriangles[triangleIndex + 1] = i == CircleSegments - 1 ? 1 : i + 2;
                circleTriangles[triangleIndex + 2] = i + 1;
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

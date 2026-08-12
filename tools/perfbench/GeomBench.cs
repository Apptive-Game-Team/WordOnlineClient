// SkillIndicatorShapeRenderer의 도형 생성 경로 구/신 비교.
// 기하 코드는 각각 git HEAD~1 / HEAD에서 그대로 복사했고, Unity 의존만 치환:
//  - Vector2/Vector3/Mathf → 동일 시그니처 스텁
//  - transform.TransformPoint/InverseTransformPoint → 위치+yaw 회전 스텁 (원 케이스는 identity 회전과 동일)
//  - Mesh 업로드 → 정점/삼각형 버퍼 구성까지만 측정 (GPU 업로드 비용은 측정 불가로 별도 명시)
using System;
using System.Collections.Generic;

namespace Bench.Geom
{
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
    }

    public static class Mathf
    {
        public const float PI = (float)Math.PI;
        public static float Cos(float v) => (float)Math.Cos(v);
        public static float Sin(float v) => (float)Math.Sin(v);
    }

    // 위치 + Y축 회전만 있는 트랜스폼 (인디케이터 오브젝트의 실제 사용 범위와 동일)
    public class TransformStub
    {
        public Vector3 position;
        public float yawRad;

        public Vector3 TransformPoint(Vector3 local)
        {
            float c = (float)Math.Cos(yawRad), s = (float)Math.Sin(yawRad);
            return new Vector3(
                position.x + local.x * c + local.z * s,
                position.y + local.y,
                position.z - local.x * s + local.z * c);
        }

        public Vector3 InverseTransformPoint(Vector3 world)
        {
            float c = (float)Math.Cos(yawRad), s = (float)Math.Sin(yawRad);
            float dx = world.x - position.x, dy = world.y - position.y, dz = world.z - position.z;
            return new Vector3(dx * c - dz * s, dy, dx * s + dz * c);
        }
    }

    // ───────────────────────── 수정 전 (HEAD~1) ─────────────────────────
    public class OldShapeBuilder
    {
        private const int CircleSegments = 64;
        private static readonly Vector2 FieldMin = new Vector2(0f, 0f);
        private static readonly Vector2 FieldMax = new Vector2(18f, 10f);
        private static readonly Color DefaultFillColor = new Color(0.95f, 0.62f, 0.18f, 0.24f);

        public TransformStub transform = new TransformStub();

        public int LastVertexCount;

        public void BuildCircleMesh(float radius)
        {
            var polygon = new List<Vector2>(CircleSegments);
            for (int i = 0; i < CircleSegments; i++)
            {
                float angle = Mathf.PI * 2f * i / CircleSegments;
                polygon.Add(new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }

            List<Vector2> clipped = ClipPolygonToFieldInWorldSpace(polygon);

            if (clipped == null || clipped.Count < 3)
            {
                LastVertexCount = 0;
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

            LastVertexCount = vertCount + tris.Length + colors.Length; // dead-code 제거 방지
        }

        private List<Vector2> ClipPolygonToFieldInWorldSpace(List<Vector2> localPolygon)
        {
            var worldPolygon = new List<Vector2>(localPolygon.Count);
            foreach (Vector2 local in localPolygon)
            {
                Vector3 worldPos = transform.TransformPoint(new Vector3(local.x, 0f, local.y));
                worldPolygon.Add(new Vector2(worldPos.x, worldPos.z));
            }

            List<Vector2> clippedWorld = ClipPolygonToAABB(worldPolygon, FieldMin, FieldMax);
            if (clippedWorld == null || clippedWorld.Count < 3) return null;

            float worldY = transform.position.y;
            var result = new List<Vector2>(clippedWorld.Count);
            foreach (Vector2 world in clippedWorld)
            {
                Vector3 localPos = transform.InverseTransformPoint(new Vector3(world.x, worldY, world.y));
                result.Add(new Vector2(localPos.x, localPos.z));
            }
            return result;
        }

        private static List<Vector2> ClipPolygonToAABB(List<Vector2> polygon, Vector2 min, Vector2 max)
        {
            if (polygon == null || polygon.Count < 3) return null;

            List<Vector2> output = polygon;

            output = ClipByEdge(output, p => p.x >= min.x, (a, b) =>
            {
                float t = (min.x - a.x) / (b.x - a.x);
                return new Vector2(min.x, a.y + t * (b.y - a.y));
            });
            if (output == null || output.Count < 3) return null;

            output = ClipByEdge(output, p => p.x <= max.x, (a, b) =>
            {
                float t = (max.x - a.x) / (b.x - a.x);
                return new Vector2(max.x, a.y + t * (b.y - a.y));
            });
            if (output == null || output.Count < 3) return null;

            output = ClipByEdge(output, p => p.y >= min.y, (a, b) =>
            {
                float t = (min.y - a.y) / (b.y - a.y);
                return new Vector2(a.x + t * (b.x - a.x), min.y);
            });
            if (output == null || output.Count < 3) return null;

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
    }

    // ───────────────────────── 수정 후 (HEAD) ─────────────────────────
    public class NewShapeBuilder
    {
        private const int CircleSegments = 64;
        private static readonly Vector2 FieldMin = new Vector2(0f, 0f);
        private static readonly Vector2 FieldMax = new Vector2(18f, 10f);
        private static readonly Color DefaultFillColor = new Color(0.95f, 0.62f, 0.18f, 0.24f);

        public TransformStub transform = new TransformStub();

        private readonly List<Vector2> polygonBuffer = new List<Vector2>(CircleSegments);
        private readonly List<Vector2> clipBufferA = new List<Vector2>(CircleSegments + 8);
        private readonly List<Vector2> clipBufferB = new List<Vector2>(CircleSegments + 8);
        private readonly List<Vector3> vertexBuffer = new List<Vector3>(CircleSegments + 8);
        private readonly List<Color> colorBuffer = new List<Color>(CircleSegments + 8);
        private readonly List<int> triangleBuffer = new List<int>((CircleSegments + 8) * 3);

        // dirty check (SetCircle 수준의 동일-입력 생략 재현)
        private Vector3 lastPosition;
        private float lastSize;
        private bool hasBuiltShape;

        public int LastVertexCount;
        public int SkippedByDirtyCheck;

        public void SetCircle(Vector3 position, float radius)
        {
            if (hasBuiltShape &&
                lastPosition.x == position.x && lastPosition.y == position.y && lastPosition.z == position.z &&
                lastSize == radius)
            {
                SkippedByDirtyCheck++;
                return;
            }

            lastPosition = position;
            lastSize = radius;
            hasBuiltShape = true;

            transform.position = position;
            BuildCircleMesh(radius);
        }

        private void BuildCircleMesh(float radius)
        {
            polygonBuffer.Clear();
            for (int i = 0; i < CircleSegments; i++)
            {
                float angle = Mathf.PI * 2f * i / CircleSegments;
                polygonBuffer.Add(new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }

            ApplyPolygonToMesh(ClipPolygonToFieldInWorldSpace(polygonBuffer));
        }

        private void ApplyPolygonToMesh(List<Vector2> polygon)
        {
            if (polygon == null || polygon.Count < 3)
            {
                LastVertexCount = 0;
                return;
            }

            int vertCount = polygon.Count;
            vertexBuffer.Clear();
            colorBuffer.Clear();
            triangleBuffer.Clear();

            for (int i = 0; i < vertCount; i++)
            {
                Vector2 point = polygon[i];
                vertexBuffer.Add(new Vector3(point.x, 0f, point.y));
                colorBuffer.Add(DefaultFillColor);
            }

            for (int i = 0; i < vertCount - 2; i++)
            {
                triangleBuffer.Add(0);
                triangleBuffer.Add(i + 2);
                triangleBuffer.Add(i + 1);
            }

            LastVertexCount = vertexBuffer.Count + triangleBuffer.Count + colorBuffer.Count;
        }

        private List<Vector2> ClipPolygonToFieldInWorldSpace(List<Vector2> localPolygon)
        {
            int count = localPolygon.Count;
            if (count < 3)
            {
                return null;
            }

            clipBufferA.Clear();
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;
            for (int i = 0; i < count; i++)
            {
                Vector2 local = localPolygon[i];
                Vector3 worldPos = transform.TransformPoint(new Vector3(local.x, 0f, local.y));
                clipBufferA.Add(new Vector2(worldPos.x, worldPos.z));

                if (worldPos.x < minX) minX = worldPos.x;
                if (worldPos.x > maxX) maxX = worldPos.x;
                if (worldPos.z < minZ) minZ = worldPos.z;
                if (worldPos.z > maxZ) maxZ = worldPos.z;
            }

            if (minX >= FieldMin.x && maxX <= FieldMax.x && minZ >= FieldMin.y && maxZ <= FieldMax.y)
            {
                return localPolygon;
            }

            List<Vector2> clippedWorld = ClipPolygonToAABB(clipBufferA, clipBufferB);
            if (clippedWorld == null)
            {
                return null;
            }

            float worldY = transform.position.y;
            for (int i = 0; i < clippedWorld.Count; i++)
            {
                Vector2 world = clippedWorld[i];
                Vector3 localPos = transform.InverseTransformPoint(new Vector3(world.x, worldY, world.y));
                clippedWorld[i] = new Vector2(localPos.x, localPos.z);
            }

            return clippedWorld;
        }

        private static List<Vector2> ClipPolygonToAABB(List<Vector2> input, List<Vector2> scratch)
        {
            if (input == null || input.Count < 3)
            {
                return null;
            }

            List<Vector2> source = input;
            List<Vector2> target = scratch;

            for (int edge = 0; edge < 4; edge++)
            {
                target.Clear();
                ClipByEdge(source, target, edge);
                if (target.Count < 3)
                {
                    return null;
                }

                List<Vector2> swap = source;
                source = target;
                target = swap;
            }

            return source;
        }

        private static void ClipByEdge(List<Vector2> input, List<Vector2> output, int edge)
        {
            int count = input.Count;
            if (count == 0)
            {
                return;
            }

            Vector2 previous = input[count - 1];
            bool prevInside = IsInsideEdge(previous, edge);

            for (int i = 0; i < count; i++)
            {
                Vector2 current = input[i];
                bool curInside = IsInsideEdge(current, edge);

                if (curInside)
                {
                    if (!prevInside)
                    {
                        output.Add(IntersectEdge(previous, current, edge));
                    }
                    output.Add(current);
                }
                else if (prevInside)
                {
                    output.Add(IntersectEdge(previous, current, edge));
                }

                previous = current;
                prevInside = curInside;
            }
        }

        private static bool IsInsideEdge(Vector2 point, int edge)
        {
            switch (edge)
            {
                case 0: return point.x >= FieldMin.x;
                case 1: return point.x <= FieldMax.x;
                case 2: return point.y >= FieldMin.y;
                default: return point.y <= FieldMax.y;
            }
        }

        private static Vector2 IntersectEdge(Vector2 a, Vector2 b, int edge)
        {
            switch (edge)
            {
                case 0:
                {
                    float t = (FieldMin.x - a.x) / (b.x - a.x);
                    return new Vector2(FieldMin.x, a.y + t * (b.y - a.y));
                }
                case 1:
                {
                    float t = (FieldMax.x - a.x) / (b.x - a.x);
                    return new Vector2(FieldMax.x, a.y + t * (b.y - a.y));
                }
                case 2:
                {
                    float t = (FieldMin.y - a.y) / (b.y - a.y);
                    return new Vector2(a.x + t * (b.x - a.x), FieldMin.y);
                }
                default:
                {
                    float t = (FieldMax.y - a.y) / (b.y - a.y);
                    return new Vector2(a.x + t * (b.x - a.x), FieldMax.y);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using GameScene.Dto.debug;
using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class ServedObjectGizmoRenderer : MonoBehaviour
    {
        private const int GizmoCircleSegments = 40;
        private const float GizmoLineWidth = 0.035f;
        private const float GizmoZOffsetStep = 0.01f;

        private static Material _gizmoLineMaterial;

        [SerializeField] private ServedObject servedObject;

        private readonly List<Gizmo> _gizmos = new List<Gizmo>();
        private readonly List<LineRenderer> _gizmoRenderers = new List<LineRenderer>();
        private Transform _gizmoContainer;
        private SpriteRenderer _spriteRenderer;

        public IReadOnlyList<Gizmo> Gizmos => _gizmos;

        private void Awake()
        {
            if (servedObject == null)
            {
                servedObject = GetComponent<ServedObject>();
            }
        }

        public void SetGizmos(List<Gizmo> gizmos)
        {
            _gizmos.Clear();
            if (gizmos != null)
            {
                _gizmos.AddRange(gizmos);
            }

            RebuildGizmoRenderers();
        }

        private void RebuildGizmoRenderers()
        {
            EnsureGizmoContainer();
            ClearGizmoRenderers();

            for (int i = 0; i < _gizmos.Count; i++)
            {
                Gizmo gizmo = _gizmos[i];
                if (gizmo == null)
                {
                    continue;
                }

                CreateGizmoRenderer(gizmo, i);
            }
        }

        private void EnsureGizmoContainer()
        {
            if (_gizmoContainer != null)
            {
                return;
            }

            GameObject gizmoContainerObject = new GameObject("DebugGizmos");
            _gizmoContainer = gizmoContainerObject.transform;
            _gizmoContainer.SetParent(GetAnchorTransform(), false);
            _gizmoContainer.localPosition = Vector3.zero;
            _gizmoContainer.localRotation = Quaternion.identity;
            _gizmoContainer.localScale = Vector3.one;
        }

        private Transform GetAnchorTransform()
        {
            if (servedObject != null)
            {
                return servedObject.GetActualTransform();
            }

            return transform;
        }

        private void ClearGizmoRenderers()
        {
            foreach (LineRenderer renderer in _gizmoRenderers)
            {
                if (renderer != null)
                {
                    Destroy(renderer.gameObject);
                }
            }

            _gizmoRenderers.Clear();
        }

        private void CreateGizmoRenderer(Gizmo gizmo, int index)
        {
            Debug.Log("[Gizmo] Creating Gizmo Renderer");
            GameObject gizmoObject = new GameObject(GetGizmoObjectName(gizmo, index));
            gizmoObject.transform.SetParent(_gizmoContainer, false);

            LineRenderer lineRenderer = gizmoObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.widthMultiplier = GizmoLineWidth;
            lineRenderer.positionCount = 0;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.alignment = LineAlignment.TransformZ;
            lineRenderer.sharedMaterial = GetGizmoLineMaterial();
            ApplyGizmoSorting(lineRenderer, index);
            lineRenderer.startColor = GetGizmoColor(gizmo.category);
            lineRenderer.endColor = lineRenderer.startColor;

            WDebug.Log($"[Gizmo] Creating gizmo renderer for gizmo at relative position {gizmo.relativePosition}, type: {gizmo.type}, category: {gizmo.category}");
            Vector3[] points = BuildGizmoPoints(gizmo, index);
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);

            _gizmoRenderers.Add(lineRenderer);
        }

        private static string GetGizmoObjectName(Gizmo gizmo, int index)
        {
            string category = string.IsNullOrWhiteSpace(gizmo.category) ? "Unknown" : gizmo.category;
            string type = string.IsNullOrWhiteSpace(gizmo.type) ? "Shape" : gizmo.type;
            return $"Gizmo_{index}_{category}_{type}";
        }

        private void ApplyGizmoSorting(LineRenderer lineRenderer, int index)
        {
            EnsureSpriteRenderer();
            if (_spriteRenderer != null)
            {
                lineRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
                lineRenderer.sortingOrder = _spriteRenderer.sortingOrder + 20 + index;
                return;
            }

            lineRenderer.sortingLayerName = "Default";
            lineRenderer.sortingOrder = 500 + index;
        }

        private void EnsureSpriteRenderer()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private static Material GetGizmoLineMaterial()
        {
            if (_gizmoLineMaterial != null)
            {
                return _gizmoLineMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            _gizmoLineMaterial = new Material(shader);
            return _gizmoLineMaterial;
        }

        private static Color GetGizmoColor(string category)
        {
            switch (category)
            {
                case "Collider":
                    return new Color(0.27f, 0.87f, 0.44f, 0.95f);
                case "AttackRange":
                    return new Color(0.95f, 0.28f, 0.28f, 0.95f);
                case "AreaOfEffect":
                    return new Color(0.98f, 0.58f, 0.17f, 0.95f);
                case "DetectionRange":
                    return new Color(0.28f, 0.75f, 1f, 0.95f);
                case "SpawnArea":
                    return new Color(0.72f, 0.41f, 0.97f, 0.95f);
                default:
                    return new Color(1f, 0.95f, 0.35f, 0.95f);
            }
        }

        private static Vector3[] BuildGizmoPoints(Gizmo gizmo, int index)
        {
            Vector3 center = gizmo.relativePosition + Vector3.forward * (index * GizmoZOffsetStep);
            if (string.Equals(gizmo.type, "Box", StringComparison.OrdinalIgnoreCase))
            {
                return BuildBoxPoints(center, gizmo.boxSize);
            }

            return BuildCirclePoints(center, gizmo.radius);
        }

        private static Vector3[] BuildCirclePoints(Vector3 center, float radius)
        {
            float safeRadius = Mathf.Max(radius, 0.05f);
            Vector3[] points = new Vector3[GizmoCircleSegments];

            for (int i = 0; i < GizmoCircleSegments; i++)
            {
                float angle = Mathf.PI * 2f * i / GizmoCircleSegments;
                float x = Mathf.Cos(angle) * safeRadius;
                float y = Mathf.Sin(angle) * safeRadius;
                points[i] = center + new Vector3(x, y, 0f);
            }

            return points;
        }

        private static Vector3[] BuildBoxPoints(Vector3 center, Vector3 boxSize)
        {
            WDebug.Log($"[Gizmo] Building box points for gizmo at {center} with size {boxSize}");
            Vector3 safeSize = new Vector3(
                Mathf.Max(boxSize.x, 0.1f),
                Mathf.Max(boxSize.y, 0.1f),
                Mathf.Max(boxSize.z, 0f)
            );

            Vector3 half = safeSize * 0.5f;
            return new[]
            {
                center + new Vector3(-half.x, -half.y, 0f),
                center + new Vector3(-half.x, half.y, 0f),
                center + new Vector3(half.x, half.y, 0f),
                center + new Vector3(half.x, -half.y, 0f),
            };
        }

        private void OnDestroy()
        {
            ClearGizmoRenderers();
        }
    }
}

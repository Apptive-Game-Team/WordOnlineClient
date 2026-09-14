using System.Collections.Generic;
using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    /// <summary>
    /// 서버가 보낸 DetectionRange gizmo 반경을 따라 바닥 표식 조각을 여러 개 깔아, 감시 범위를
    /// 테두리 선 없이 보여 준다.
    /// <para>
    /// 한 장짜리 타원 그림 대신 조각을 반복하는 이유는 두 가지다. 반경이 바뀌어도 배치 지름만
    /// 달라지고 그림은 늘어나지 않으며, `.art/STYLE.md`가 금지하는 테두리 선 없이 형태가 읽힌다.
    /// </para>
    /// <para>
    /// 감시 반경을 가진 모든 객체가 아니라 이 component를 붙인 prefab에서만 그린다. zap_mouse나
    /// fire_tadpole처럼 detection_range를 쓰는 mob이 많아서, 자동으로 붙이면 화면이 전부 덮인다.
    /// </para>
    /// </summary>
    public class DetectionRangeMarker : ServedObjectBehaviour
    {
        private const string MarkerRootName = "DetectionRangeMarker";
        private const string DetectionRangeCategory = "DetectionRange";
        private const int MinimumMarkerCount = 4;
        private const int MaximumMarkerCount = 24;

        [SerializeField] private string markerSpriteResourcePath = "Game/field/shock_trap_range_marker";

        /// <summary>바닥 표식이므로 몸통(AbstractBuild의 10)보다 아래, ElectricField와 같은 9에 둔다.</summary>
        [SerializeField] private int sortingOrder = 9;

        /// <summary>조각 하나의 가로 폭, world 단위. 반경이 커져도 조각 크기는 그대로 두고 개수만 늘린다.</summary>
        [SerializeField] private float markerWorldWidth = 1.3f;

        /// <summary>이웃한 두 조각의 중심 사이 거리, world 단위. 조각 폭보다 커야 사이가 벌어진다.</summary>
        [SerializeField] private float markerSpacing = 1.9f;

        /// <summary>
        /// 서버가 gizmo를 보내지 않은 생성(동기화 복구 등)에서 쓸 반경. shock_trap의 서버 radius가 3이다.
        /// </summary>
        [SerializeField] private float fallbackRadius = 3f;

        private readonly List<GameObject> markers = new List<GameObject>();

        protected override void OnBound()
        {
            float radius = fallbackRadius;
            if (Owner != null && Owner.TryGetGizmoRadius(DetectionRangeCategory, out float gizmoRadius))
            {
                radius = gizmoRadius;
            }

            if (radius <= 0f)
            {
                return;
            }

            Sprite markerSprite = Resources.Load<Sprite>(markerSpriteResourcePath);
            if (markerSprite == null)
            {
                WDebug.LogWarning($"Detection range marker sprite '{markerSpriteResourcePath}' not found.");
                return;
            }

            BuildMarkers(markerSprite, radius);
        }

        private void BuildMarkers(Sprite markerSprite, float radius)
        {
            Transform markerRoot = new GameObject(MarkerRootName).transform;
            markerRoot.SetParent(transform, false);
            markerRoot.localPosition = Vector3.zero;
            markerRoot.localRotation = Quaternion.identity;

            int markerCount = GetMarkerCount(radius);
            float scale = GetMarkerScale(markerSprite);

            for (int i = 0; i < markerCount; i++)
            {
                float angle = Mathf.PI * 2f * i / markerCount;
                GameObject marker = new GameObject($"{MarkerRootName}_{i}");
                marker.transform.SetParent(markerRoot, false);

                // 땅은 XZ 평면이다 (SkillIndicatorShapeRenderer.BuildCircleEdge와 같은 규칙).
                marker.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius);
                marker.transform.localRotation = Quaternion.identity;
                marker.transform.localScale = new Vector3(scale, scale, 1f);

                SpriteRenderer markerRenderer = marker.AddComponent<SpriteRenderer>();
                markerRenderer.sprite = markerSprite;
                markerRenderer.sortingOrder = sortingOrder;

                // 같은 그림을 돌려 쓰는 티가 나지 않게 한 칸씩 좌우를 뒤집는다.
                markerRenderer.flipX = (i % 2) == 1;

                markers.Add(marker);
            }
        }

        /// <summary>둘레를 <see cref="markerSpacing"/>으로 나눠 개수를 정한다. 반경이 커지면 개수만 늘어난다.</summary>
        private int GetMarkerCount(float radius)
        {
            float spacing = Mathf.Max(markerSpacing, 0.01f);
            int count = Mathf.RoundToInt(2f * Mathf.PI * radius / spacing);
            return Mathf.Clamp(count, MinimumMarkerCount, MaximumMarkerCount);
        }

        /// <summary>
        /// 조각의 가로 폭을 <see cref="markerWorldWidth"/>에 맞춘다. 그림의 world 폭은 PPU에서 나오므로,
        /// 부모가 이미 배율을 갖고 있으면 그만큼 나눠 준다.
        /// </summary>
        private float GetMarkerScale(Sprite markerSprite)
        {
            float spriteWidth = markerSprite.bounds.size.x;
            if (spriteWidth <= 0f)
            {
                return 1f;
            }

            float parentWidthScale = transform.lossyScale.x;
            if (Mathf.Approximately(parentWidthScale, 0f))
            {
                parentWidthScale = 1f;
            }

            return markerWorldWidth / (spriteWidth * parentWidthScale);
        }
    }
}

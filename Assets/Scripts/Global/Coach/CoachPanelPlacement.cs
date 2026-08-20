using System;
using UnityEngine;

namespace Global.Coach
{
    /// <summary>
    /// 훈수 힌트가 화면 어디에 앉을지. TutorialPanel은 온보딩에 맞춘 위치를 하드코딩하는데,
    /// 온보딩은 필드를 가려도 되지만 비차단 힌트는 그러면 안 된다. 그래서 director가
    /// 배치를 직접 잡는다.
    /// </summary>
    [Serializable]
    public struct CoachPanelPlacement
    {
        [Tooltip("캔버스 기준 정규화 anchor. (0.5, 1)이 상단 중앙, (0, 0)이 좌측 하단이다.")]
        public Vector2 anchor;

        [Tooltip("패널 자신의 pivot. 보통 anchor와 같게 둔다.")]
        public Vector2 pivot;

        [Tooltip("anchor에서 밀어낼 거리. 캔버스 단위다.")]
        public Vector2 offset;

        public void ApplyTo(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            target.anchorMin = anchor;
            target.anchorMax = anchor;
            target.pivot = pivot;
            target.anchoredPosition = offset;
        }

        /// <summary>상단 중앙. 손패와 마나 바, 필드를 모두 비켜난 자리다.</summary>
        public static CoachPanelPlacement TopCentre => new CoachPanelPlacement
        {
            anchor = new Vector2(0.5f, 1f),
            pivot = new Vector2(0.5f, 1f),
            offset = new Vector2(0f, -40f)
        };

        /// <summary>상단 우측. 대상이 왼쪽에 있는 힌트가 쓴다.</summary>
        public static CoachPanelPlacement TopRight => new CoachPanelPlacement
        {
            anchor = new Vector2(1f, 1f),
            pivot = new Vector2(1f, 1f),
            offset = new Vector2(-40f, -40f)
        };
    }
}

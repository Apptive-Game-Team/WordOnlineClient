using System;
using UnityEngine;

namespace Global.Coach
{
    /// <summary>
    /// Where a coach hint sits on screen. TutorialPanel hard-codes positions
    /// tuned for the onboarding flow, which is free to cover the field; a
    /// non-blocking hint is not, so the director places the panel itself.
    /// </summary>
    [Serializable]
    public struct CoachPanelPlacement
    {
        [Tooltip("Normalized anchor on the canvas. (0.5, 1) is top centre, (0, 0) is bottom left.")]
        public Vector2 anchor;

        [Tooltip("Pivot of the panel itself, usually matching the anchor.")]
        public Vector2 pivot;

        [Tooltip("Offset from the anchor, in canvas units.")]
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

        /// <summary>Top centre, clear of the hand, the mana bar and the field.</summary>
        public static CoachPanelPlacement TopCentre => new CoachPanelPlacement
        {
            anchor = new Vector2(0.5f, 1f),
            pivot = new Vector2(0.5f, 1f),
            offset = new Vector2(0f, -40f)
        };

        /// <summary>Top right, for hints whose target sits on the left.</summary>
        public static CoachPanelPlacement TopRight => new CoachPanelPlacement
        {
            anchor = new Vector2(1f, 1f),
            pivot = new Vector2(1f, 1f),
            offset = new Vector2(-40f, -40f)
        };
    }
}

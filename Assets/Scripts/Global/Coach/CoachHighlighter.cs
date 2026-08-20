using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Global.Coach
{
    /// <summary>
    /// Pulses an outline around whatever a hint points at. Anything it touches
    /// is restored exactly as it was, because targets such as CardUI own an
    /// Outline of their own that the game toggles independently.
    /// </summary>
    public class CoachHighlighter : MonoBehaviour
    {
        private class Captured
        {
            public Outline Outline;
            public bool AddedByUs;
            public bool WasEnabled;
            public Color OriginalColor;
            public Vector2 OriginalDistance;
            public Tween Pulse;
        }

        [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.3f, 1f);
        [SerializeField] private Vector2 outlineDistance = new Vector2(4f, -4f);
        [SerializeField] private float pulseDuration = 0.6f;
        [SerializeField] private float pulseMinAlpha = 0.25f;

        private readonly List<Captured> captured = new List<Captured>();

        public void Highlight(Transform[] targets)
        {
            Clear();

            if (targets == null)
            {
                return;
            }

            foreach (Transform target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                Capture(target);
            }
        }

        public void Clear()
        {
            foreach (Captured entry in captured)
            {
                entry.Pulse?.Kill();

                if (entry.Outline == null)
                {
                    continue;
                }

                if (entry.AddedByUs)
                {
                    Destroy(entry.Outline);
                    continue;
                }

                entry.Outline.effectColor = entry.OriginalColor;
                entry.Outline.effectDistance = entry.OriginalDistance;
                entry.Outline.enabled = entry.WasEnabled;
            }

            captured.Clear();
        }

        private void Capture(Transform target)
        {
            Graphic graphic = target.GetComponent<Graphic>();
            if (graphic == null)
            {
                // Outline is a UI effect, so it cannot decorate a plain transform.
                return;
            }

            Outline outline = target.GetComponent<Outline>();
            bool addedByUs = outline == null;
            if (addedByUs)
            {
                outline = target.gameObject.AddComponent<Outline>();
            }

            Captured entry = new Captured
            {
                Outline = outline,
                AddedByUs = addedByUs,
                WasEnabled = !addedByUs && outline.enabled,
                OriginalColor = outline.effectColor,
                OriginalDistance = outline.effectDistance
            };

            outline.enabled = true;
            outline.effectColor = highlightColor;
            outline.effectDistance = outlineDistance;

            Color dimmed = highlightColor;
            dimmed.a = pulseMinAlpha;
            entry.Pulse = DOTween
                .To(() => outline.effectColor, value => outline.effectColor = value, dimmed, pulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);

            captured.Add(entry);
        }

        private void OnDisable()
        {
            Clear();
        }
    }
}

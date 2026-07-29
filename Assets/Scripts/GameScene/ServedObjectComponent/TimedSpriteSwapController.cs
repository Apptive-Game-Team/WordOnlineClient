using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace GameScene.ServedObjectComponent
{
    /// <summary>
    /// Shows a second drawn frame for a fixed duration, then returns to the idle frame.
    /// <para>
    /// Only <see cref="SpriteRenderer.sprite"/> changes, so the facing set by
    /// <see cref="ServedObject.SetMaster"/> and any idle motion controller are left alone. The
    /// swapped frame is expected to be trimmed from the same canvas as the idle frame and to carry
    /// a pivot that puts it back on the idle frame's anchor.
    /// </para>
    /// Leave <see cref="swapSprite"/> empty on prefabs that have no extra frame — the component
    /// then costs one null check at bind time and never subscribes.
    /// </summary>
    public abstract class TimedSpriteSwapController : ServedObjectBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Sprite swapSprite;
        [SerializeField] private float duration = 0.1f;

        private SpriteRenderer resolvedRenderer;
        private Sprite idleSprite;
        private Sequence sequence;

        protected override void OnBound()
        {
            if (swapSprite == null)
            {
                return;
            }

            resolvedRenderer = ResolveRenderer(targetRenderer);
            if (resolvedRenderer == null)
            {
                return;
            }

            idleSprite = resolvedRenderer.sprite;
            Subscribe(Owner);
        }

        protected override void OnUnbound()
        {
            if (Owner != null)
            {
                Unsubscribe(Owner);
            }

            StopAndRestore();
        }

        private void OnDisable()
        {
            StopAndRestore();
        }

        /// <summary>Hook the event that should trigger the swap.</summary>
        protected abstract void Subscribe(ServedObject owner);

        protected abstract void Unsubscribe(ServedObject owner);

        /// <summary>Plays the swap. Safe to call again while a previous swap is still running.</summary>
        protected void Play()
        {
            if (resolvedRenderer == null || swapSprite == null)
            {
                return;
            }

            StopAndRestore();
            idleSprite = resolvedRenderer.sprite;
            resolvedRenderer.sprite = swapSprite;

            Sequence swapSequence = DOTween.Sequence();
            sequence = swapSequence;
            swapSequence.SetLink(gameObject);
            swapSequence
                .AppendInterval(duration)
                .AppendCallback(RestoreIdle)
                .OnComplete(() => ClearSequence(swapSequence))
                .OnKill(() =>
                {
                    ClearSequence(swapSequence);
                    RestoreIdle();
                });
        }

        private void ClearSequence(Sequence finished)
        {
            if (sequence == finished)
            {
                sequence = null;
            }
        }

        private void StopAndRestore()
        {
            Sequence activeSequence = sequence;
            sequence = null;
            if (activeSequence != null && activeSequence.IsActive())
            {
                activeSequence.Kill(false);
            }

            RestoreIdle();
        }

        private void RestoreIdle()
        {
            if (resolvedRenderer != null && idleSprite != null)
            {
                resolvedRenderer.sprite = idleSprite;
            }
        }
    }
}

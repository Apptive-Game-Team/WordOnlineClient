using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace GameScene.ServedObjectComponent.Effect
{
    /// <summary>
    /// Moves the staff aura anchor to follow the staff tip through the body's attack frame swap.
    /// <para>
    /// The aura is drawn as a separate sprite so it can pulse independently, which means it must be
    /// repositioned by hand on every frame swap rather than moving with the body sprite. The pulsing
    /// itself lives on the child sprite's <see cref="IdleAuraEffect"/> so the two do not fight over
    /// the same transform.
    /// </para>
    /// </summary>
    public sealed class PlayerStaffAuraController : ServedObjectBehaviour
    {
        [SerializeField] private Transform auraAnchor;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Vector3 idleLocalPosition;
        [SerializeField] private Vector3 attackLocalPosition;
        [SerializeField] private float attackDuration = 0.18f;

        private Sequence sequence;
        private Vector3 currentLocalPosition;
        private bool lastFlipX;

        protected override void OnBound()
        {
            if (auraAnchor == null || bodyRenderer == null || Owner == null)
            {
                return;
            }

            lastFlipX = bodyRenderer.flipX;
            ApplyPosition(idleLocalPosition);
            Owner.OnAttack += Play;
        }

        protected override void OnUnbound()
        {
            if (Owner != null)
            {
                Owner.OnAttack -= Play;
            }

            StopAndRestore();
        }

        private void OnDisable()
        {
            StopAndRestore();
        }

        private void Update()
        {
            if (bodyRenderer == null || bodyRenderer.flipX == lastFlipX)
            {
                return;
            }

            lastFlipX = bodyRenderer.flipX;
            ApplyPosition(currentLocalPosition);
        }

        private void Play()
        {
            if (auraAnchor == null || bodyRenderer == null)
            {
                return;
            }

            StopSequence();
            ApplyPosition(attackLocalPosition);

            Sequence attackSequence = DOTween.Sequence();
            sequence = attackSequence;
            attackSequence.SetLink(gameObject);
            attackSequence
                .AppendInterval(attackDuration)
                .AppendCallback(RestoreIdle)
                .OnComplete(() => ClearSequence(attackSequence))
                .OnKill(() =>
                {
                    ClearSequence(attackSequence);
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
            StopSequence();
            RestoreIdle();
        }

        private void StopSequence()
        {
            Sequence activeSequence = sequence;
            sequence = null;
            if (activeSequence != null && activeSequence.IsActive())
            {
                activeSequence.Kill(false);
            }
        }

        private void RestoreIdle()
        {
            ApplyPosition(idleLocalPosition);
        }

        private void ApplyPosition(Vector3 localPosition)
        {
            currentLocalPosition = localPosition;
            if (auraAnchor == null)
            {
                return;
            }

            Vector3 mirroredPosition = localPosition;
            if (bodyRenderer != null && bodyRenderer.flipX)
            {
                mirroredPosition.x *= -1;
            }

            auraAnchor.localPosition = mirroredPosition;
        }
    }
}

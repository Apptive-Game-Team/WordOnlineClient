using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace GameScene.ServedObjectComponent.Effect
{
    [DisallowMultipleComponent]
    public class SpawnPopInEffect : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        [Min(0f)]
        [SerializeField] private float startScaleMultiplier = 0.15f;
        [Min(0f)]
        [SerializeField] private float duration = 0.28f;
        [SerializeField] private Ease ease = Ease.OutBack;

        private Sequence sequence;
        private Vector3 targetScale;

        private void Awake()
        {
            if (targetTransform == null)
            {
                targetTransform = transform;
            }

            Play();
        }

        private void OnDestroy()
        {
            Stop();
        }

        public void Play()
        {
            Stop();

            if (targetTransform == null)
            {
                return;
            }

            targetScale = targetTransform.localScale;
            targetTransform.localScale = targetScale * Mathf.Max(0f, startScaleMultiplier);

            if (duration <= 0f)
            {
                targetTransform.localScale = targetScale;
                return;
            }

            sequence = DOTween.Sequence();
            sequence.SetLink(gameObject);
            sequence
                .Append(targetTransform.DOScale(targetScale, duration).SetEase(ease))
                .OnKill(() => sequence = null);
        }

        private void Stop()
        {
            sequence?.Kill();
            sequence = null;
        }
    }
}

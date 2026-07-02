using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace GameScene.ServedObjectComponent.Effect
{
    public class AttackAuraEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float contractScale = 0.86f;
        [SerializeField] private float burstScale = 1.35f;
        [SerializeField] private float duration = 0.3f;

        private Sequence sequence;

        private void Start()
        {
            Play();
        }

        private void OnDestroy()
        {
            sequence?.Kill();
            sequence = null;
        }

        private void Play()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 baseScale = transform.localScale;
            float baseAlpha = spriteRenderer.color.a;

            sequence = DOTween.Sequence();
            sequence.SetLink(gameObject);
            sequence
                .Append(transform.DOScale(baseScale * contractScale, duration * 0.35f)
                    .SetEase(Ease.InSine))
                .Append(transform.DOScale(baseScale * burstScale, duration * 0.65f)
                    .SetEase(Ease.OutCubic))
                .Join(DOTween.To(
                    () => spriteRenderer.color.a,
                    SetAlpha,
                    0f,
                    duration * 0.65f).SetEase(Ease.OutCubic))
                .OnComplete(() =>
                {
                    SetAlpha(0f);
                    Destroy(gameObject);
                });

            SetAlpha(baseAlpha);
        }

        private void SetAlpha(float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}

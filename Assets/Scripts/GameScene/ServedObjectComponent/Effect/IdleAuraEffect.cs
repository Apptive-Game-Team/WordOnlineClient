using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace GameScene.ServedObjectComponent.Effect
{
    public class IdleAuraEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float pulseScale = 1.08f;
        [SerializeField] private float squashScale = 0.94f;
        [SerializeField] private float cycleDuration = 1.2f;
        [SerializeField] private float rotationAngle = 3f;
        [SerializeField] private float minAlphaMultiplier = 0.7f;

        private Sequence scaleSequence;
        private Tween alphaTween;
        private Tween rotationTween;
        private bool initialized;

        private void Start()
        {
            initialized = true;
            Play();
        }

        private void OnEnable()
        {
            if (initialized)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }

        private void Play()
        {
            Stop();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                return;
            }

            Vector3 baseScale = transform.localScale;
            float baseAlpha = spriteRenderer.color.a;

            scaleSequence = DOTween.Sequence();
            scaleSequence.SetLink(gameObject);
            scaleSequence
                .Append(transform.DOScale(
                    new Vector3(baseScale.x * pulseScale, baseScale.y * squashScale, baseScale.z),
                    cycleDuration * 0.33f).SetEase(Ease.InOutSine))
                .Append(transform.DOScale(
                    new Vector3(baseScale.x * squashScale, baseScale.y * pulseScale, baseScale.z),
                    cycleDuration * 0.34f).SetEase(Ease.InOutSine))
                .Append(transform.DOScale(baseScale, cycleDuration * 0.33f).SetEase(Ease.InOutSine))
                .SetLoops(-1);

            alphaTween = DOTween.To(
                    () => spriteRenderer.color.a,
                    SetAlpha,
                    baseAlpha * minAlphaMultiplier,
                    cycleDuration * 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject);

            rotationTween = transform
                .DOLocalRotate(new Vector3(0f, 0f, rotationAngle), cycleDuration * 0.5f)
                .From(new Vector3(0f, 0f, -rotationAngle))
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject);
        }

        private void Stop()
        {
            scaleSequence?.Kill();
            scaleSequence = null;
            alphaTween?.Kill();
            alphaTween = null;
            rotationTween?.Kill();
            rotationTween = null;
        }

        private void SetAlpha(float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}

using DG.Tweening;
using UnityEngine;

namespace GameScene.ServedObjectComponent.Effect
{
    /// <summary>
    /// 덫이 터진 자리에서 한 번 퍼졌다 사라진다.
    /// <para>
    /// 목표 크기는 이 component가 정하지 않는다. <c>GameEventHandler</c>가 서버 반경에 맞춰 넣어 둔
    /// <see cref="Transform.localScale"/>을 Start에서 그대로 읽어 목표로 삼고, 그보다 작게 시작해
    /// 거기까지 커진다. Instantiate 직후 같은 frame에 크기가 정해지고 Start는 다음 frame에 도는
    /// 순서라서 이 읽기가 성립한다.
    /// </para>
    /// <para>
    /// 줄어들며 사라지는 <c>ShrinkSelfDestroyer</c>를 쓰지 않은 이유는, 전기가 퍼져서 주변을 stun
    /// 시키는 장면인데 오므라들면 반대로 읽히기 때문이다.
    /// </para>
    /// </summary>
    public class ShockBurstEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        /// <summary>목표 크기 대비 시작 크기. 0에서 시작하면 한 frame 동안 점으로 보인다.</summary>
        [SerializeField] private float startScaleRatio = 0.45f;

        /// <summary>목표 크기까지 커지는 시간(초).</summary>
        [SerializeField] private float growDuration = 0.1f;

        /// <summary>다 커진 뒤 남은 시간(초). 투명해지는 건 커지는 동안 이미 시작한다.</summary>
        [SerializeField] private float fadeDuration = 0.16f;

        private Sequence sequence;

        private void Start()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            Vector3 targetScale = transform.localScale;
            transform.localScale = targetScale * Mathf.Clamp01(startScaleRatio);

            sequence = DOTween.Sequence();
            sequence.SetLink(gameObject);
            sequence.Append(transform.DOScale(targetScale, growDuration).SetEase(Ease.OutQuad));

            if (spriteRenderer != null)
            {
                sequence.Join(DOTween.To(
                        () => spriteRenderer.color.a,
                        SetAlpha,
                        0f,
                        growDuration + fadeDuration)
                    .SetEase(Ease.InQuad));
            }

            sequence.OnComplete(() => Destroy(gameObject));
        }

        private void OnDestroy()
        {
            sequence?.Kill();
            sequence = null;
        }

        private void SetAlpha(float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}

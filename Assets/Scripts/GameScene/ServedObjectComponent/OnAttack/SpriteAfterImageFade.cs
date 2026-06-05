using System;
using System.Collections;
using UnityEngine;

namespace GameScene.ServedObjectComponent.OnAttack
{
    public class SpriteAfterImageFade : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Coroutine fadeCoroutine;
        private Action<SpriteAfterImageFade> onComplete;

        public void Play(float lifetime, float endScaleMultiplier, Action<SpriteAfterImageFade> onComplete)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            this.onComplete = onComplete;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeRoutine(lifetime, endScaleMultiplier));
        }

        private IEnumerator FadeRoutine(float lifetime, float endScaleMultiplier)
        {
            if (lifetime <= 0f)
            {
                Complete();
                yield break;
            }

            Vector3 startScale = transform.localScale;
            Vector3 endScale = startScale * Mathf.Max(0f, endScaleMultiplier);
            Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            float elapsed = 0f;

            while (elapsed < lifetime)
            {
                float progress = elapsed / lifetime;
                transform.localScale = Vector3.Lerp(startScale, endScale, progress);

                if (spriteRenderer != null)
                {
                    Color color = startColor;
                    color.a = Mathf.Lerp(startColor.a, 0f, progress);
                    spriteRenderer.color = color;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Complete();
        }

        private void Complete()
        {
            fadeCoroutine = null;
            onComplete?.Invoke(this);
        }
    }
}

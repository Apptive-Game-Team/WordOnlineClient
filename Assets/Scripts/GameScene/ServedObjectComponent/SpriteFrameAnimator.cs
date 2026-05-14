using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class SpriteFrameAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float frameInterval = 0.12f;
        [SerializeField] private bool loop = true;

        private int frameIndex;
        private float elapsed;

        private void Awake()
        {
            ApplyFrame(0);
        }

        private void Update()
        {
            if (frames == null || frames.Length <= 1 || frameInterval <= 0f)
            {
                return;
            }

            elapsed += Time.deltaTime;
            while (elapsed >= frameInterval)
            {
                elapsed -= frameInterval;
                int nextIndex = frameIndex + 1;
                if (nextIndex >= frames.Length)
                {
                    nextIndex = loop ? 0 : frames.Length - 1;
                }

                ApplyFrame(nextIndex);
            }
        }

        private void ApplyFrame(int index)
        {
            if (!spriteRenderer || frames == null || frames.Length == 0)
            {
                return;
            }

            frameIndex = Mathf.Clamp(index, 0, frames.Length - 1);
            Sprite frame = frames[frameIndex];
            if (frame)
            {
                spriteRenderer.sprite = frame;
            }
        }
    }
}

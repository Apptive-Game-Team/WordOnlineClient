using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScene.Object
{
    public sealed class TitanFistPresenter : MonoBehaviour
    {
        private const float TelegraphDuration = 0.12f;
        private const float RiseDuration = 0.18f;
        private const float HoldDuration = 0.28f;
        private const float LowerDuration = 0.28f;
        private readonly List<LineRenderer> cracks = new List<LineRenderer>();

        private SpriteRenderer fistRenderer;
        private Transform fistTransform;
        private Vector3 fullScale;
        private Material crackMaterial;

        public static TitanFistPresenter Attach(GameObject target)
        {
            TitanFistPresenter existing = target.GetComponent<TitanFistPresenter>();
            return existing != null ? existing : target.AddComponent<TitanFistPresenter>();
        }

        private void Awake()
        {
            fistRenderer = GetComponentInChildren<SpriteRenderer>();
            if (fistRenderer == null)
            {
                enabled = false;
                return;
            }

            fistTransform = fistRenderer.transform;
            fullScale = fistTransform.localScale;
            fistRenderer.enabled = false;
            CreateGroundCracks();
        }

        private void Start()
        {
            if (enabled)
            {
                StartCoroutine(Play());
            }
        }

        private IEnumerator Play()
        {
            SetCrackAlpha(0f);
            yield return FadeCracks(0f, 1f, TelegraphDuration);
            fistRenderer.enabled = true;
            yield return ScaleFist(0.06f, 1f, RiseDuration);
            yield return new WaitForSeconds(HoldDuration);
            yield return ScaleFist(1f, 0.06f, LowerDuration);
            fistRenderer.enabled = false;
            yield return FadeCracks(1f, 0f, 0.12f);
        }

        private IEnumerator ScaleFist(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                float y = Mathf.Lerp(from, to, t);
                fistTransform.localScale = new Vector3(fullScale.x, fullScale.y * y, fullScale.z);
                yield return null;
            }

            fistTransform.localScale = new Vector3(fullScale.x, fullScale.y * to, fullScale.z);
        }

        private void CreateGroundCracks()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return;
            }

            crackMaterial = new Material(shader);
            Vector3 center = transform.position + Vector3.up * 0.025f;
            for (int index = 0; index < 6; index++)
            {
                float angle = index * Mathf.PI * 2f / 6f + 0.18f;
                float length = index % 2 == 0 ? 0.8f : 0.58f;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

                GameObject crackObject = new GameObject($"Crack{index}");
                crackObject.transform.SetParent(transform, true);
                LineRenderer line = crackObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = 3;
                line.SetPosition(0, center + direction * 0.18f);
                line.SetPosition(1, center + direction * (length * 0.62f)
                    + new Vector3(-direction.z, 0f, direction.x) * 0.1f);
                line.SetPosition(2, center + direction * length);
                line.startWidth = 0.075f;
                line.endWidth = 0.025f;
                line.numCapVertices = 1;
                line.material = crackMaterial;
                line.sortingOrder = 11;
                cracks.Add(line);
            }
        }

        private IEnumerator FadeCracks(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetCrackAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            SetCrackAlpha(to);
        }

        private void SetCrackAlpha(float alpha)
        {
            Color color = new Color(0.22f, 0.18f, 0.12f, alpha);
            foreach (LineRenderer crack in cracks)
            {
                crack.startColor = color;
                crack.endColor = color;
            }
        }

        private void OnDestroy()
        {
            if (crackMaterial != null)
            {
                Destroy(crackMaterial);
            }
        }
    }
}

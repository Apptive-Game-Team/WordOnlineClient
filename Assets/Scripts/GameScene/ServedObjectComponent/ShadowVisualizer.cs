using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class ShadowVisualizer : MonoBehaviour
    {
        private const string ShadowObjectName = "Shadow";

        [SerializeField] private Transform shadowTransform;
        [SerializeField] private SpriteRenderer shadowRenderer;
        [SerializeField] private float groundY = 0f;
        [SerializeField] private float maxHeight = 5f;
        [SerializeField] private float scaleGrowthPerHeight = 0.12f;
        [SerializeField] private float minAlphaMultiplier = 0.2f;

        private Vector3 baseLocalScale;
        private float baseAlpha;

        private void Awake()
        {
            if (shadowTransform == null)
            {
                shadowTransform = FindChildByName(transform, ShadowObjectName);
            }

            if (shadowTransform != null && shadowRenderer == null)
            {
                shadowRenderer = shadowTransform.GetComponent<SpriteRenderer>();
            }

            CacheBaseValues();
        }

        private void LateUpdate()
        {
            if (shadowTransform == null)
            {
                return;
            }

            float height = Mathf.Max(0f, transform.position.y - groundY);
            float heightRate = maxHeight > Mathf.Epsilon ? Mathf.Clamp01(height / maxHeight) : 0f;

            shadowTransform.position = new Vector3(transform.position.x, groundY, transform.position.z);
            shadowTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
            shadowTransform.localScale = baseLocalScale * (1f + height * scaleGrowthPerHeight);

            if (shadowRenderer == null)
            {
                return;
            }

            Color color = shadowRenderer.color;
            float alphaMultiplier = Mathf.Lerp(1f, minAlphaMultiplier, heightRate);
            color.a = baseAlpha * alphaMultiplier;
            shadowRenderer.color = color;
        }

        private void CacheBaseValues()
        {
            if (shadowTransform != null)
            {
                baseLocalScale = shadowTransform.localScale;
            }

            if (shadowRenderer != null)
            {
                baseAlpha = shadowRenderer.color.a;
            }
        }

        private static Transform FindChildByName(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            foreach (Transform child in root)
            {
                Transform result = FindChildByName(child, childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}

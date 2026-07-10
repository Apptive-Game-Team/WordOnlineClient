using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class ZVisualizer : SimpleZVisualizer
    {
        private const float SHADOW_DEFAULT_ALPHA = 0.8f;
        private const float SHADOW_CONSTANT = 5f;
    
        [SerializeField] private SpriteRenderer ShadowSpriteRenderer;

        private float Z => transform.position.y;

        private void Update()
        {
            UpdateVisualGameObject();
            UpdateShadowGameObject();
        }

        private void UpdateShadowGameObject()
        {
            Color shadowColor = ShadowSpriteRenderer.color;
            shadowColor.a = Mathf.Clamp((SHADOW_CONSTANT - Z) / SHADOW_CONSTANT, 0.1f, SHADOW_DEFAULT_ALPHA);
            ShadowSpriteRenderer.color = shadowColor;
        }
    }
}


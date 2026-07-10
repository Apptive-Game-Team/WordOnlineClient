using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class ChickenCommandoSpriteSwap : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite elevatedSprite;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private float heightThreshold = 0.5f;

        private void Update()
        {
            if (!spriteRenderer)
            {
                return;
            }

            Sprite targetSprite = transform.position.y > heightThreshold ? elevatedSprite : defaultSprite;
            if (targetSprite != null && spriteRenderer.sprite != targetSprite)
            {
                spriteRenderer.sprite = targetSprite;
            }
        }
    }
}

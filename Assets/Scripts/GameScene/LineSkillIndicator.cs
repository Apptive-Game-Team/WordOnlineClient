using UnityEngine;

namespace GameScene
{
    public class LineSkillIndicator : MonoBehaviour
    {
        private const int SkillIndicatorSortingOrder = 14;

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private SpriteRenderer spriteRenderer;
        private SkillIndicatorShapeRenderer shapeRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            shapeRenderer = GetComponent<SkillIndicatorShapeRenderer>();
            if (shapeRenderer == null)
            {
                shapeRenderer = gameObject.AddComponent<SkillIndicatorShapeRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }

        public void SetIndicator(Vector3 startPosition, Vector3 targetPosition, float range, float radius)
        {
            this.startPosition = startPosition;
            this.targetPosition = targetPosition;
            if (shapeRenderer != null)
            {
                shapeRenderer.SetLine(startPosition, targetPosition, range, SkillIndicatorSortingOrder, radius * 2f);
                return;
            }

            transform.position = startPosition;
            transform.localScale = new Vector3(GetScaleForLength(range), transform.localScale.y, transform.localScale.z);
            UpdateRotation();
        }

        private float GetScaleForLength(float length)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null || length <= 0f)
            {
                return 0f;
            }

            return length / spriteRenderer.sprite.bounds.size.x;
        }

        private void UpdateRotation()
        {
            Vector3 direction = targetPosition - startPosition;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(90f, -angle, 0f);
        }
    }
}

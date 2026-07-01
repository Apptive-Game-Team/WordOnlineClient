using UnityEngine;

namespace GameScene
{
    public class CircleSkillIndicator : MonoBehaviour
    {
        private const int SkillIndicatorSortingOrder = 14;

        [SerializeField] private float groundDiameterAtScaleOne = 1f;
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

        public void SetIndicator(Vector3 position, float radius)
        {
            if (shapeRenderer != null)
            {
                shapeRenderer.SetCircle(position, radius, true, SkillIndicatorSortingOrder, 0f);
                return;
            }

            transform.position = position;
            transform.localScale = GetScaleForRadius(radius);
        }

        private Vector3 GetScaleForRadius(float radius)
        {
            if (radius <= 0f)
            {
                return new Vector3(0f, 0f, 1f);
            }

            float groundDiameter = GetGroundDiameterAtScaleOne();
            float targetDiameter = radius * 2f;
            float scale = targetDiameter / groundDiameter;
            return new Vector3(scale, scale, 1f);
        }

        private float GetGroundDiameterAtScaleOne()
        {
            if (groundDiameterAtScaleOne > 0f)
            {
                return groundDiameterAtScaleOne;
            }

            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return 1f;
            }

            return Mathf.Max(spriteRenderer.sprite.bounds.size.x, Mathf.Epsilon);
        }
    }
}

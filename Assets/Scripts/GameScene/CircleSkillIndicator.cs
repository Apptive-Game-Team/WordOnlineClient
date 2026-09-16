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

        /// <param name="edgeWidth">
        /// 테두리 선 굵기. 기본값 0f는 테두리 없이 채우기만 그리던 기존 동작 그대로다.
        /// 0보다 큰 값을 주면 <see cref="SkillIndicatorShapeRenderer.SetCircle"/>이 테두리를 그려서,
        /// 같은 채우기색을 쓰는 다른 원형 indicator와 선 굵기로 구분할 수 있다.
        /// </param>
        public void SetIndicator(Vector3 position, float radius, float edgeWidth = 0f)
        {
            if (shapeRenderer != null)
            {
                shapeRenderer.SetCircle(position, radius, true, SkillIndicatorSortingOrder, edgeWidth);
                return;
            }

            transform.position = position;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
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

using System.Collections;
using GameScene.ServedObjectComponent;
using UnityEngine;

namespace GameScene.Animation
{
    public class OnAttackSpriteSwapper: MonoBehaviour
    {
        
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite onAttackSprite;
        [SerializeField] private ServedObject servedObject;
        [SerializeField] private float animationDuration = 0.2f;
        
        private Sprite _defaultSprite;

        private void Awake()
        {
            _defaultSprite = spriteRenderer.sprite;
        }
        
        private void OnEnable()
        {
            servedObject.OnAttack += SwapSprite;
        }

        private void OnDisable()
        {
            servedObject.OnAttack -= SwapSprite;
        }

        private void SwapSprite()
        {
            StartCoroutine(SwapSpriteCoroutine());
        }
        
        private IEnumerator SwapSpriteCoroutine()
        {
            spriteRenderer.sprite = onAttackSprite;
            yield return new WaitForSeconds(animationDuration);
            spriteRenderer.sprite = _defaultSprite;
        }
    }
}
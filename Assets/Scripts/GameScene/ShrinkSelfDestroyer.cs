using GameScene.ServedObjectComponent;
using UnityEngine;

namespace GameScene
{
    public class ShrinkSelfDestroyer : MonoBehaviour
    {
    
        [SerializeField] protected float duration = 0.5f;

        protected virtual void Start()
        {
            DOTweenAction
                .Shrink(transform, duration, () =>
                {
                    transform.localScale = Vector3.zero;
                });
            Destroy(gameObject, duration);
        }
    }
}
using GameScene.ServedObjectComponent;
using UnityEngine;

namespace GameScene
{
    public class PopSelfController : MonoBehaviour
    {
    
        [SerializeField] protected float duration = 0.5f;
    
        private void Awake()
        {
            transform.localScale = Vector3.zero;
        }

        protected virtual void Start()
        {
            DOTweenAction
                .Pop(transform, duration, () =>
                {
                    transform.localScale = Vector3.zero;
                });
        }
    }
}
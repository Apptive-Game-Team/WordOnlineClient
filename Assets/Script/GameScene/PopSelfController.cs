using UnityEngine;

namespace Script.GameScene
{
    public class PopSelfController : MonoBehaviour
    {
    
        protected const float Duration = 0.5f;
    
        private void Awake()
        {
            transform.localScale = Vector3.zero;
        }

        protected virtual void Start()
        {
            DOTweenAction
                .Pop(transform, Duration);
        }
    }
}
using UnityEngine;

namespace Script.GameScene
{
    public class PopSelfDestroyer : MonoBehaviour
    {
    
        private const float Duration = 0.5f;
    
        private void Awake()
        {
            transform.localScale = Vector3.zero;
        }
    
        private void Start()
        {
            DOTweenAction
                .Pop(transform, Duration);
            Destroy(gameObject, Duration);
        }
    }
}

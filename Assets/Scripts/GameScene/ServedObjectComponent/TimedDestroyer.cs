using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class TimedDestroyer : MonoBehaviour
    {
        [SerializeField] public float lifetime = 5f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}

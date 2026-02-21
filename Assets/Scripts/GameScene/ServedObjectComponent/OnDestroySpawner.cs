using UnityEngine;

namespace Scripts.GameScene.ServedObjectComponent
{
    public class OnDestroySpawner : MonoBehaviour
    {
        
        public GameObject prefab;

        private void OnDestroy()
        {
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }
    }
}
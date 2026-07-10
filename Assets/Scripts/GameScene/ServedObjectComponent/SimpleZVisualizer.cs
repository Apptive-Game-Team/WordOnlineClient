using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class SimpleZVisualizer : MonoBehaviour
    {
    
        [SerializeField] protected GameObject visualGameObject;
    
        public Transform ActualTransform => visualGameObject.transform;
    
        private void Update()
        {
            UpdateVisualGameObject();
        }
        protected void UpdateVisualGameObject()
        {
            visualGameObject.transform.position = CalculateZAppliedPosition(transform.position);
        }
    
        public static Vector3 CalculateZAppliedPosition(Vector3 position)
        {
            return position;
        }
    }
}


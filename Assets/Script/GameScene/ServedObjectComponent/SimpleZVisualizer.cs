using UnityEngine;

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
        Vector3 visualPosition = position;
        visualPosition.y += position.z / 2;
        return visualPosition;
    }
}


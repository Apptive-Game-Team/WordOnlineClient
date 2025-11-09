using UnityEngine;

public class ZVisualizer : MonoBehaviour
{
    private const float SHADOW_DEFAULT_ALPHA = 0.8f;
    private const float SHADOW_CONSTANT = 5f;
    
    private GameObject visualGameObject;
    private SpriteRenderer ShadowSpriteRenderer;

    private float Z
    {
        get { return transform.position.z; }
    }

    private void Awake()
    {
        visualGameObject = transform.GetChild(1).gameObject;
        ShadowSpriteRenderer = transform.GetChild(2).GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        UpdateVisualGameObject();
        UpdateShadowGameObject();
    }

    private void UpdateShadowGameObject()
    {
        Color shadowColor = ShadowSpriteRenderer.color;
        shadowColor.a = Mathf.Clamp((SHADOW_CONSTANT - Z) / SHADOW_CONSTANT, 0.1f, SHADOW_DEFAULT_ALPHA);
        ShadowSpriteRenderer.color = shadowColor;
    }

    private void UpdateVisualGameObject()
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


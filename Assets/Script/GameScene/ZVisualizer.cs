using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZVisualizer : MonoBehaviour
{
    private const float SHADOW_DEFAULT_ALPHA = 0.8f;
    
    private GameObject visualGameObject;
    private SpriteRenderer ShadowSpriteRenderer;

    private float Z
    {
        get { return transform.position.z; }
    }

    private void Awake()
    {
        visualGameObject = transform.GetChild(0).gameObject;
        ShadowSpriteRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        UpdateVisualGameObject();
        UpdateShadowGameObject();
    }

    private void UpdateShadowGameObject()
    {
        Color shadowColor = ShadowSpriteRenderer.color;
        shadowColor.a = SHADOW_DEFAULT_ALPHA * (1 - Z); 
        Debug.Log("Z: " + Z + ", Alpha: " + shadowColor.a);
        ShadowSpriteRenderer.color = shadowColor;
    }

    private void UpdateVisualGameObject()
    {
        Vector3 visualPosition = transform.position;
        visualPosition.y += Z / 2;
        visualGameObject.transform.position = visualPosition;
    }
}


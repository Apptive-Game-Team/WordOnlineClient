using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferFlipXFlipper : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _referRenderer;
    private SpriteRenderer _renderer;
    [SerializeField] private Vector3 initialPosition;
    
    private void Awake()
    {
        if (_referRenderer == null)
        {
            Destroy(this);
        }
        _renderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.localPosition;
    }

    private void Update()
    {
        bool flipX = _referRenderer.flipX;

        if (flipX != _renderer.flipX)
        {
            _renderer.flipX = flipX;
            if (flipX)
            {
                Vector3 currentVect = initialPosition;
                currentVect.x *= -1;
                transform.localPosition = currentVect;
            }
            else
            {
                transform.localPosition = initialPosition;
            }
        }
    }
}

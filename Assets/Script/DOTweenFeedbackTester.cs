using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOTweenFeedbackTester : MonoBehaviour
{
    [SerializeField] private Transform obj1;
    [SerializeField] private Transform obj2;
    [SerializeField] private Transform obj3;

    struct BounceTesterParameters
    {
        public Vector3 originScale;
        public float squashScale;
        public float bounceScale;
        public float duration;
    }
    
    BounceTesterParameters bounce = new BounceTesterParameters
    {
        originScale = Vector3.one,
        squashScale = 0.8f,
        bounceScale = 1.2f,
        duration    = 0.2f
    };
    
    struct SwingTesterParameters
    {
        public float angle;
        public float duration;
    }
    
    SwingTesterParameters swing = new SwingTesterParameters
    {
        angle = 30f,
        duration  = 0.4f
    };
    
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DOTweenAction.DOBounce(obj1, bounce.originScale, bounce.squashScale, bounce.bounceScale, bounce.duration);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            DOTweenAction.DOSwing(obj2, swing.angle, swing.duration);
        }
    }
}

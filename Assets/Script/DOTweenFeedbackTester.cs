using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DOTweenFeedbackTester : MonoBehaviour
{
    [SerializeField] private Transform obj1;
    [SerializeField] private Transform obj2;
    [SerializeField] private Transform playerObj;

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
            DOTweenAction.Bounce(obj1, bounce.originScale, bounce.squashScale, bounce.bounceScale, bounce.duration);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            DOTweenAction.Swing(obj2, swing.angle, swing.duration);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            DOTweenAction.Bounce(playerObj, bounce.originScale, bounce.squashScale, bounce.bounceScale, bounce.duration);
            DOTweenAction.Rotate(playerObj,swing.angle, swing.duration, RotateMode.LocalAxisAdd);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            DOTweenAction.Rotate(playerObj,0, swing.duration);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DOTweenAction.CrawlMob(obj1.transform);
        }
    }
}

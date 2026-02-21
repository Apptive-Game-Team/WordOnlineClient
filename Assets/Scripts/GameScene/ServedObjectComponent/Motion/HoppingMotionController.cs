using UnityEngine;

namespace Scripts.GameScene.ServedObjectComponent.Motion
{
    public class HoppingMotionController : DOTweenMotionController
    {
        [SerializeField] private float _duration = 0.5f;
        protected override void Awake()
        { 
            DOTweenAction.HopMob(appliedTransform, _duration);
        }
    }
}
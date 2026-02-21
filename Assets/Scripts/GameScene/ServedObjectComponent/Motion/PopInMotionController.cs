using UnityEngine;

namespace GameScene.ServedObjectComponent.Motion
{
    public class PopInMotionController : DOTweenMotionController
    {
        [SerializeField] float _duration = 1f;
        protected override void Awake()
        {
           DOTweenAction.PopIn(appliedTransform, _duration);  
        }
    }
}
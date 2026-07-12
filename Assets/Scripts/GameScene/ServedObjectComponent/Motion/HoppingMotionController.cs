using UnityEngine;

namespace GameScene.ServedObjectComponent.Motion
{
    public class HoppingMotionController : DOTweenMotionController
    {
        [SerializeField] private float _duration = 0.5f;
        protected override void Awake()
        { 
            DOTweenAction.HopMob(appliedTransform, _duration);
            StartMotionSound(MotionSoundProfile.Hop, _duration * 2f, _duration * 2f);
        }
    }
}

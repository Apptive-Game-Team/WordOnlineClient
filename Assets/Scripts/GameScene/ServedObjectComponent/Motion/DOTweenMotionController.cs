using UnityEngine;

namespace GameScene.ServedObjectComponent.Motion
{
    public abstract class DOTweenMotionController : MonoBehaviour
    {
        [SerializeField] protected Transform appliedTransform;
        protected abstract void Awake();
    }
}
using System;
using UnityEngine;
namespace Script.GameScene
{
    public abstract class DOTweenMotionController : MonoBehaviour
    {
        [SerializeField] protected Transform appliedTransform;
        protected abstract void Awake();
    }
}
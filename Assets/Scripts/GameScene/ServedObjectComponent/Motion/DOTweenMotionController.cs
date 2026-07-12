using UnityEngine;

namespace GameScene.ServedObjectComponent.Motion
{
    public abstract class DOTweenMotionController : MonoBehaviour
    {
        [SerializeField] protected Transform appliedTransform;

        protected void StartMotionSound(MotionSoundProfile profile, float initialDelay, float interval)
        {
            MotionSoundPlayer player = gameObject.GetComponent<MotionSoundPlayer>();
            if (player == null)
            {
                player = gameObject.AddComponent<MotionSoundPlayer>();
            }

            player.Play(profile, initialDelay, interval);
        }

        protected abstract void Awake();
    }
}

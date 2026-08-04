using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent.Effect
{
    public class HitEffectController : MonoBehaviour
    {
        [SerializeField] private ServedObject servedObject;
        // how far toward the attacker the star lands, in world units. Tune per prefab.
        [SerializeField] private float hitDirectionOffset = 0.25f;

        private Vector3 pendingHitDirection;

        /// <summary>
        /// Marks where the next hit came from, so the star lands on the struck side of the body
        /// instead of the center. Called from the frame's hit event, which arrives in the same
        /// message as the HP drop that plays the effect.
        /// </summary>
        public static void NotifyHit(ServedObject attacker, ServedObject target)
        {
            HitEffectController controller = target.GetComponentInChildren<HitEffectController>();
            if (controller == null)
            {
                return;
            }

            controller.pendingHitDirection =
                (attacker.transform.position - target.transform.position).normalized;
        }

        private void Start()
        {
            if (servedObject == null)
            {
                servedObject = GetComponentInParent<ServedObject>();
            }

            if (servedObject != null)
            {
                servedObject.OnHpDecreased += PlayHitEffect;
            }
        }
        
        private void OnDestroy()
        {
            if (servedObject != null)
            {
                servedObject.OnHpDecreased -= PlayHitEffect;
            }
        }
        
        private void PlayHitEffect()
        {
            WDebug.Log($"Hit effect played for ServedObject ID: {servedObject.id}");
            DamagedObjectEffect.SetSelfDestroyEffect("HitEffect", transform, pendingHitDirection * hitDirectionOffset);
            pendingHitDirection = Vector3.zero;
            DOTweenAction.BounceMob(transform);
        }
    }
}
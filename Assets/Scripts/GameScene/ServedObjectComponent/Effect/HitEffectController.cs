using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent.Effect
{
    /// <summary>
    /// Two reactions to taking damage, driven by different signals.
    /// <para>
    /// The recoil follows any HP drop, including sources with no attacker such as damage over time.
    /// The star only plays for a server <c>hit</c> event, which is what tells us where the blow came
    /// from, and it is placed on the side of the sprite facing the attacker.
    /// </para>
    /// </summary>
    public class HitEffectController : MonoBehaviour
    {
        [SerializeField] private ServedObject servedObject;

        [Tooltip("How far toward the attacker the star sits, as a fraction of the sprite's half size.")]
        [SerializeField, Range(0f, 1f)] private float attackerSideBias = 0.6f;

        private void Start()
        {
            if (servedObject == null)
            {
                servedObject = GetComponentInParent<ServedObject>();
            }

            if (servedObject != null)
            {
                servedObject.OnHpDecreased += PlayRecoil;
            }
        }

        private void OnDestroy()
        {
            if (servedObject != null)
            {
                servedObject.OnHpDecreased -= PlayRecoil;
            }
        }

        public void PlayHitFrom(Vector3 attackerWorldPosition)
        {
            PlayHitFrom(attackerWorldPosition, "HitEffect");
        }

        public void PlayHitFrom(Vector3 attackerWorldPosition, string effectName)
        {
            if (servedObject == null)
            {
                PlayHit(effectName);
                return;
            }

            WDebug.Log($"Hit effect played for ServedObject ID: {servedObject.id}");
            DamagedObjectEffect.SetSelfDestroyEffect(
                effectName,
                servedObject.GetEdgeWorldPositionTowards(attackerWorldPosition, attackerSideBias));
        }

        /// <summary>Used when the attacker is no longer on the client, so there is no side to favour.</summary>
        public void PlayHit()
        {
            PlayHit("HitEffect");
        }

        private void PlayHit(string effectName)
        {
            DamagedObjectEffect.SetSelfDestroyEffect(effectName, transform.position);
        }

        private void PlayRecoil()
        {
            DOTweenAction.BounceMob(transform);
        }
    }
}

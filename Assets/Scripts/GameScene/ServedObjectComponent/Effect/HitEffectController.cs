using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent.Effect
{
    public class HitEffectController : MonoBehaviour
    {
        [SerializeField] private ServedObject servedObject;

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
            DamagedObjectEffect.SetSelfDestroyEffect("HitEffect",transform);
            DOTweenAction.BounceMob(transform);
        }
    }
}
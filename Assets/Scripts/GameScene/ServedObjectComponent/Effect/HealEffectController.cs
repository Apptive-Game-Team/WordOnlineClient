using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent.Effect
{
    public class HealEffectController : MonoBehaviour
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
                servedObject.OnHpIncreased += PlayHealEffect;
            }
        }
        
        private void OnDestroy()
        {
            if (servedObject != null)
            {
                servedObject.OnHpIncreased -= PlayHealEffect;
            }
        }
        
        private void PlayHealEffect()
        {
            WDebug.Log($"Hit effect played for ServedObject ID: {servedObject.id}");
            DamagedObjectEffect.SetSelfDestroyEffect("HealEffect",transform);
        }
    }
}
using GameScene.ServedObjectComponent;
using Global;
using UnityEngine;

namespace GameScene.Player
{
    public class PlayerActionController : MonoBehaviour
    {
    
        [SerializeField] private ServedObject servedObject;
        [SerializeField] private MagicFailEffecter magicFailEffecter;
        private GameObject playerObject;
        private Transform playerTransform;
    
        private void Start()
        {
            if (servedObject == null)
            {
                servedObject = GetComponent<ServedObject>();
            }

            playerTransform = servedObject != null ? servedObject.GetActualTransform() : transform;
            if (servedObject == null)
            {
                WDebug.LogWarning($"{nameof(PlayerActionController)} requires a {nameof(ServedObject)}.");
                return;
            }

            servedObject.OnOtherStatus += OnOtherStatus;
            servedObject.OnAttack += OnAttack;
        }

        private void OnDestroy()
        {
            if (servedObject == null) return;
            
            servedObject.OnOtherStatus -= OnOtherStatus;
            servedObject.OnAttack -= OnAttack;
        }
        
        private void OnAttack()
        {
            if (playerTransform == null && servedObject != null)
            {
                playerTransform = servedObject.GetActualTransform();
            }

            if (playerTransform == null)
            {
                WDebug.LogWarning($"{nameof(PlayerActionController)} attack animation skipped because target transform is missing.");
                return;
            }

            DOTweenAction.SwingMobAttack(playerTransform);
        }

        private void OnOtherStatus(string status)
        {
            if (status.Equals("Hindered"))
            {
                magicFailEffecter?.Trigger();
            }
        }
    }
}

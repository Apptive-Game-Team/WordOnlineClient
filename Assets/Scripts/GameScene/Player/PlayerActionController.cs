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
            DOTweenAction.SwingMobAttack(playerTransform);
        }

        private void OnOtherStatus(string status)
        {
            if (status.Equals("Hindered"))
            {
                magicFailEffecter.Trigger();
            }
        }
    }
}

using GameScene.ServedObjectComponent;
using Global;
using UnityEngine;

namespace GameScene.Player
{
    public class PlayerActionController : MonoBehaviour
    {
    
        [SerializeField] private ServedObject servedObject;
        private GameObject playerObject;
        private Transform playerTransform;
        private MagicFailEffecter magicFailEffecter;
    
        private void Start()
        {
            if (servedObject.GetMaster().Equals(SceneContext.Me))
            {
                Destroy(this);
            }
            Init();
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

        private void Init()
        {
            playerObject = GetPlayerObject();
            playerTransform = playerObject.transform.Find("PlayerObject");
            magicFailEffecter = playerObject.GetComponentInChildren<MagicFailEffecter>();
        }
        
        private GameObject GetPlayerObject()
        {
            string master = servedObject.GetMaster();
            if (master.Equals("LeftPlayer"))
            {
                return GameObject.Find("LeftPlayer");
            }
            else if (master.Equals("RightPlayer"))
            {
                return GameObject.Find("RightPlayer");
            }

            return null;
        }
    }
}

using System;
using UnityEngine;

namespace Script.GameScene.Player
{
    public class PlayerActionController : MonoBehaviour
    {
    
        [SerializeField] private ServedObject servedObject;
        private GameObject playerObject;
        private Transform playerTransform;
        private MagicFailEffecter magicFailEffecter;
    
        private void Start()
        {
            Init();
            servedObject.OnDamaged += magicFailEffecter.Trigger;
            servedObject.OnAttack += OnAttack;
        }

        private void OnDestroy()
        {
            servedObject.OnDamaged -= magicFailEffecter.Trigger;
            servedObject.OnAttack -= OnAttack;
        }
        
        private void OnAttack()
        {
            DOTweenAction.SwingMobAttack(playerTransform);
        }

        private void Init()
        {
            playerObject = GetPlayerObject();
            playerTransform = playerObject.transform.Find("PlayerObject");
            magicFailEffecter = playerObject.GetComponentInChildren<MagicFailEffecter>();
        }
        
        private GameObject GetPlayerObject()
        {
            if (SceneContext.Me.Equals("LeftPlayer"))
            {
                return GameObject.Find("LeftPlayer");
            }
            else if (SceneContext.Me.Equals("RightPlayer"))
            {
                return GameObject.Find("RightPlayer");
            }

            return null;
        }
    }
}

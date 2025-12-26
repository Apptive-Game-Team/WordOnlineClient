using Script.Global;
using UnityEngine;

namespace Script.GameScene.Player
{
    public class OnPlayerDestroyController : MonoBehaviour
    {
        [SerializeField] private ServedObject servedObject;
        
        private void OnDestroy()
        {
            WDebug.Log("OnPlayerDestroyController OnDestroy");
            FallPlayer();
        }

        private void FallPlayer()
        {
            GetPlayerObject().GetComponentInChildren<PlayerFallDownController>().FallDown();
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
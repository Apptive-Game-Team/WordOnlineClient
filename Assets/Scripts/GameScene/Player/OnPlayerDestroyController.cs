using GameScene.ServedObjectComponent;
using Global;
using UnityEngine;

namespace GameScene.Player
{
    public class OnPlayerDestroyController : MonoBehaviour
    {
        [SerializeField] private ServedObject servedObject;
        [SerializeField] private PlayerFallDownController playerFallDownController;
        
        private void OnDestroy()
        {
            WDebug.Log("OnPlayerDestroyController OnDestroy");
            FallPlayer();
        }

        private void FallPlayer()
        {
            playerFallDownController.FallDown();
        }
    }
}
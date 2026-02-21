using GameScene.ServedObjectComponent;
using UnityEngine;

namespace GameScene.Player
{
    public class PlayerFallDownController : MonoBehaviour
    {
        [SerializeField] private Transform _player;

        public void FallDown()
        {
            DOTweenAction.FallForward(_player, 1.0f);
        }
    }
}
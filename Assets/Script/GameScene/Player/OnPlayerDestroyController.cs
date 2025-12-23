using UnityEngine;

namespace Script.GameScene.Player
{
    public class OnPlayerDestroyController : MonoBehaviour
    {
        [SerializeField] private ServedObject servedObject;
        
        private void Start()
        {
            servedObject.OnHpChanged += OnHpChanged;
            servedObject.OnDestroyed += FallPlayer;
        }
        
        private void OnDestroy()
        {
            servedObject.OnHpChanged -= OnHpChanged;
            servedObject.OnDestroyed -= FallPlayer;
        }

        private void OnHpChanged(int hp)
        {
            if (hp <= 0)
            {
                FallPlayer();
            }
        }

        private void FallPlayer()
        {
            Transform player = GetPlayerObject().transform.Find("PlayerObject");
            DOTweenAction.FallForward(player, 1.0f);
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
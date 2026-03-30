using System.Linq;
using GameScene.Player;
using Global;
using UnityEngine;

namespace GameScene.ServedObjectComponent
{
    public class PlayerFeedbackController : LocalSingletonObject<PlayerFeedbackController> 
    {
    
        private GameObject playerObject;

        public GameObject PlayerObject
        {
            get
            {
                if (playerObject == null)
                {
                    playerObject = GetPlayerObject();
                }
                return playerObject;
            }
        }
    
        private GameObject GetPlayerObject()
        {
            return FindObjectsByType<PlayerNameSetter>(FindObjectsSortMode.None)
                .Where(o => o.GetComponent<ServedObject>() != null)
                .Select(o => o.GetComponent<ServedObject>())
                .Where(s => s.GetMaster().Equals(SceneContext.Me))
                .Select(s => s.gameObject)
                .First();
        }

        public void PlayCardSelectFeedback()
        {
            Transform spriteTr = PlayerObject.transform.Find("PlayerImage");
            DOTweenAction.RotatePlayerUseCard(spriteTr);
        }
        public void CancelCardSelectFeedback()
        {
            Transform spriteTr = PlayerObject.transform.Find("PlayerImage");
            DOTweenAction.RotatePlayerCancelCard(spriteTr);
        }

        public void UseMagicFeedback()
        {
            Transform spriteTr = PlayerObject.transform.Find("PlayerImage");
            DOTweenAction.RotatePlayerUseMagic(spriteTr);
        }
    }
}

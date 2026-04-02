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
                .FirstOrDefault();
        }

        public void PlayCardSelectFeedback()
        {
            if (PlayerObject == null)
            {
                Debug.LogWarning("[PlayerFeedbackController] Could not find player object for card select feedback.");
                return;
            }
            Transform spriteTr = PlayerObject.transform.Find("PlayerImage");
            DOTweenAction.RotatePlayerUseCard(spriteTr);
        }
        public void CancelCardSelectFeedback()
        {
            if (PlayerObject == null)
            {
                Debug.LogWarning("[PlayerFeedbackController] Could not find player object for cancel feedback.");
                return;
            }
            Transform spriteTr = PlayerObject.transform.Find("PlayerImage");
            DOTweenAction.RotatePlayerCancelCard(spriteTr);
        }

        public void UseMagicFeedback()
        {
            if (PlayerObject == null)
            {
                Debug.LogWarning("[PlayerFeedbackController] Could not find player object for magic feedback.");
                return;
            }
            Transform spriteTr = PlayerObject.transform.Find("PlayerImage");
            DOTweenAction.RotatePlayerUseMagic(spriteTr);
        }
    }
}

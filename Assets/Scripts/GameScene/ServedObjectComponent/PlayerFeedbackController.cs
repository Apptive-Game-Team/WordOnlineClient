using System.Linq;
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
            return FindObjectsByType<ServedObjectWorldUI>(FindObjectsSortMode.None)
                .Where(worldUi => worldUi.ShowsPlayerName)
                .Select(worldUi => worldUi.Owner)
                .Where(servedObject => servedObject != null)
                .Where(s => s.GetMaster().Equals(SceneContext.Me))
                .Select(s => s.gameObject)
                .FirstOrDefault();
        }

        public void PlayCardSelectFeedback()
        {
            Transform spriteTr = GetPlayerVisualTransform();
            if (spriteTr == null)
            {
                Debug.LogWarning("[PlayerFeedbackController] Could not find player visual for card select feedback.");
                return;
            }

            DOTweenAction.RotatePlayerUseCard(spriteTr);
        }

        public void CancelCardSelectFeedback()
        {
            Transform spriteTr = GetPlayerVisualTransform();
            if (spriteTr == null)
            {
                Debug.LogWarning("[PlayerFeedbackController] Could not find player visual for cancel feedback.");
                return;
            }

            DOTweenAction.RotatePlayerCancelCard(spriteTr);
        }

        public void UseMagicFeedback()
        {
            Transform spriteTr = GetPlayerVisualTransform();
            if (spriteTr == null)
            {
                Debug.LogWarning("[PlayerFeedbackController] Could not find player visual for magic feedback.");
                return;
            }

            DOTweenAction.RotatePlayerUseMagic(spriteTr);
        }

        private Transform GetPlayerVisualTransform()
        {
            if (PlayerObject == null)
            {
                return null;
            }

            ServedObject servedObject = PlayerObject.GetComponent<ServedObject>();
            return servedObject != null ? servedObject.GetActualTransform() : null;
        }
    }
}

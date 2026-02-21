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

        public void PlayCardSelectFeedback()
        {
            Transform spriteTr = PlayerObject.transform.Find("PlayerObject");
            DOTweenAction.RotatePlayerUseCard(spriteTr);
        }
        public void CancelCardSelectFeedback()
        {
            Transform spriteTr = PlayerObject.transform.Find("PlayerObject");
            DOTweenAction.RotatePlayerCancelCard(spriteTr);
        }

        public void UseMagicFeedback()
        {
            Transform spriteTr = PlayerObject.transform.Find("PlayerObject");
            DOTweenAction.RotatePlayerUseMagic(spriteTr);
        }
    }
}

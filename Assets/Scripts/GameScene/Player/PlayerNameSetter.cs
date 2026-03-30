using GameScene.ServedObjectComponent;
using Global;
using TMPro;
using UnityEngine;

namespace GameScene.Player
{
    public class PlayerNameSetter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private ServedObject servedObject;

        private void Start()
        {
            string master = servedObject.GetMaster();
            switch (master)
            {
                case "LeftPlayer":
                    playerName.text = SceneContext.MatchInfo.leftUser.name;
                    break;
                case "RightPlayer":
                    playerName.text = SceneContext.MatchInfo.rightUser.name;
                    break;
                default:
                    playerName.text = "";
                    break;
            }
        }
    }
}

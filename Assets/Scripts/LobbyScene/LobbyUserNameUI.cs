using Scripts.Global;
using TMPro;
using UnityEngine;

namespace Scripts.LobbyScene
{
    public class LobbyUserNameUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI userNameText;

        public void SetUserName(string userName)
        {
            WDebug.Log($"Name : {userName}");
            userNameText.SetText(userName);
            WDebug.Log("Set UserName");
        }
    }
}

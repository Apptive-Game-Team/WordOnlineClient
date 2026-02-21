using Global;
using TMPro;
using UnityEngine;

namespace LobbyScene
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

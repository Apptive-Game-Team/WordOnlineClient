using Script.Admin.Dto;
using TMPro;
using UnityEngine;

namespace Script.Admin
{
    public class RoomUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text sessionIdText;
        [SerializeField] private TMP_Text leftUserIdText;
        [SerializeField] private TMP_Text rightUserIdText;
        [SerializeField] private TMP_Text serverUrlText;
    
        public void SetRoomInfo(RoomInfo roomInfo)
        {
            sessionIdText.text = roomInfo.sessionId;
            leftUserIdText.text = roomInfo.leftUserId.ToString();
            rightUserIdText.text = roomInfo.rightUserId.ToString();
            serverUrlText.text = roomInfo.serverUrl;
        }
    }
}

using Scripts.Admin.Dto;
using Scripts.Data;
using Scripts.Global;
using Scripts.Global.Button;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Admin
{
    public class RoomUI : ButtonBase
    {
        [SerializeField] private TMP_Text sessionIdText;
        [SerializeField] private TMP_Text leftUserIdText;
        [SerializeField] private TMP_Text rightUserIdText;
        [SerializeField] private TMP_Text serverUrlText;
        
        private RoomInfo roomInfo;
    
        public void SetRoomInfo(RoomInfo roomInfo)
        {
            this.roomInfo = roomInfo;
            sessionIdText.text = roomInfo.sessionId;
            leftUserIdText.text = roomInfo.leftUserId.ToString();
            rightUserIdText.text = roomInfo.rightUserId.ToString();
            serverUrlText.text = roomInfo.serverUrl;
        }


        protected override void OnClickButton()
        {
            SceneContext.MatchInfo = MatchedInfoDto.CreateSpectatingSession(roomInfo);
            SceneManager.LoadScene("Scenes/SpectatingScene");
        }
    }
}

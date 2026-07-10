using Admin.Dto;
using Data;
using Global;
using Global.Button;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Admin
{
    public class RoomUI : ButtonBase
    {
        private static readonly Color LobbyTextColor = new Color(0.12f, 0.12f, 0.12f);
        private static readonly Color LocalTextColor = new Color(0.05f, 0.34f, 0.70f);

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
            serverUrlText.text = FormatServerLabel(roomInfo);
            ApplySourceStyle(roomInfo);
        }

        private static string FormatServerLabel(RoomInfo roomInfo)
        {
            string sourceName = string.IsNullOrEmpty(roomInfo.sourceName) ? "Lobby" : roomInfo.sourceName;
            string serverUrl = string.IsNullOrEmpty(roomInfo.serverUrl) ? roomInfo.sourceBaseUrl : roomInfo.serverUrl;
            return $"[{sourceName}] {serverUrl}";
        }

        private void ApplySourceStyle(RoomInfo roomInfo)
        {
            Color textColor = roomInfo.isLocalSource ? LocalTextColor : LobbyTextColor;
            sessionIdText.color = textColor;
            serverUrlText.color = textColor;
        }

        protected override void OnClickButton()
        {
            SceneContext.MatchInfo = MatchedInfoDto.CreateSpectatingSession(roomInfo);
            SceneManager.LoadScene("Scenes/SpectatingScene");
        }
    }
}

using System;

namespace Scripts.LobbyScene.SettingPage
{
    [Serializable]
    public class MemberPutRequest
    {
        public string name;
        public string email;
        public string password;
        public string lastPassword;

        public MemberPutRequest(string name, string email, string password, string lastPassword)
        {
            this.name = name;
            this.email = email;
            this.password = password;
            this.lastPassword = lastPassword;
        }
    }
}
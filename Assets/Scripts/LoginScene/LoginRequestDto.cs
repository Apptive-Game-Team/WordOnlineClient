
using System;

namespace Scripts.LoginScene
{
    [Serializable]
    public class LoginRequestDto
    {
        public string email;
        public string password;

        public LoginRequestDto(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }
}

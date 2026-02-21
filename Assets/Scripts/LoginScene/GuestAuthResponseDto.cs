using System;

namespace Scripts.LoginScene
{
    [Serializable]
    public class GuestAuthResponseDto
    {
        public string jwt;
        public string password;
    }
}
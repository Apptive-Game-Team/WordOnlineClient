using System;

namespace Script.LoginScene
{
    [Serializable]
    public class GuestAuthResponseDto
    {
        public string jwt;
        public string password;
    }
}
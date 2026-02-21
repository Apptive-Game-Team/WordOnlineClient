using System;

namespace LoginScene
{
    [Serializable]
    public class GuestAuthResponseDto
    {
        public string jwt;
        public string password;
    }
}
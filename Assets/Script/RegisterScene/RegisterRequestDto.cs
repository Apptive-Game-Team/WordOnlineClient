namespace Script.RegisterScene
{
    public class RegisterRequestDto
    {
        public string email;
        public string password;
        public string name;

        public RegisterRequestDto(string email, string password, string nickname)
        {
            this.email = email;
            this.password = password;
            this.name = nickname;
        }
    }
}
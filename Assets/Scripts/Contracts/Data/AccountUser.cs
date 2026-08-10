namespace Data
{
    [System.Serializable]
    public class AccountUser
    {
        public string name;
        public string username;
        public string email;

        public string DisplayName => string.IsNullOrEmpty(username) ? name : username;
    }
}

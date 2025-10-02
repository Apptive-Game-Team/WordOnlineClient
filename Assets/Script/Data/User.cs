namespace Script.Data
{
    [System.Serializable]
    public class User
    {
        public long id;
        public long selectedDeckId;
        public string name;
        public string email;
        
        public User(AccountUser accountUser, GameUser gameUser)
        {
            this.id = gameUser.id;
            this.selectedDeckId = gameUser.selectedDeckId;
            this.name = accountUser.name;
            this.email = accountUser.email;
        }
    }
    
    [System.Serializable]
    public class AccountUser
    {
        public string name;
        public string email;
    }
    
    [System.Serializable]
    public class GameUser
    {
        public long id;
        public long selectedDeckId;
    }
}
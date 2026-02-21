namespace Data
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
        
        public User(long id, string name, string email, long selectedDeckId)
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.selectedDeckId = selectedDeckId;
        }
    }
}
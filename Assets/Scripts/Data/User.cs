namespace Data
{
    [System.Serializable]
    public class User
    {
        public long id;
        public long selectedDeckId;
        public int mmr;
        public string name;
        public string email;
        
        public User(AccountUser accountUser, GameUser gameUser)
        {
            this.id = gameUser.id;
            this.selectedDeckId = gameUser.selectedDeckId;
            this.mmr = gameUser.mmr;
            this.name = accountUser?.DisplayName;
            this.email = accountUser?.email;
        }
        
        public User(long id, string name, string email, long selectedDeckId, int mmr = 0)
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.selectedDeckId = selectedDeckId;
            this.mmr = mmr;
        }
    }
}

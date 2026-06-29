namespace Admin.Dto
{
    [System.Serializable]
    public class RoomInfo
    {
        public string sessionId;
        public long leftUserId;
        public long rightUserId;
        public string serverUrl;
        public string sourceName;
        public string sourceBaseUrl;
        public bool isLocalSource;
    }
}

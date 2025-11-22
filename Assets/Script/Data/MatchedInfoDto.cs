using Script.Data;

[System.Serializable]
public class MatchedInfoDto
{
    public string message;
    public string server;
    public User leftUser;
    public User rightUser;
    public string sessionId;
}

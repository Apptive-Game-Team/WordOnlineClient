using Script.Data;

[System.Serializable]
public class MatchedInfoDto
{
    public string message;
    public string server;
    public User leftUser;
    public User rightUser;
    public string sessionId;
    
    public static MatchedInfoDto CreateDebugSession(string sessionId, string userSide, long userId)
    {
        long userIdLeft = userSide == "left" ? userId : -1;
        long userIdRight = userSide == "right" ? userId : -1;

        return new MatchedInfoDto
        {
            message = "debug session",
            server = "http://localhost:7777",
            leftUser = new User(userIdLeft, "debugger_left", "debugger_left@team6515.com", -1),
            rightUser = new User(userIdRight, "debugger_right", "debugger_right@team6515.com", -1),
            sessionId = sessionId
        };
    }
}

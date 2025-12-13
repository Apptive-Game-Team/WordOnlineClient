using Script.Admin.Dto;
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

    public static MatchedInfoDto CreateSpectatingSession(RoomInfo roomInfo)
    {
        return new MatchedInfoDto
        {
            message = "spectating session",
            server = roomInfo.serverUrl,
            leftUser = new User(roomInfo.leftUserId, "debugger_left", "debugger_left@team6515.com", -1),
            rightUser = new User(roomInfo.rightUserId, "debugger_right", "debugger_right@team6515.com", -1),
            sessionId = roomInfo.sessionId
        };

    }
}

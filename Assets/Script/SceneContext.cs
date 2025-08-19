using Script.Data;
using UnityEngine;

public class SceneContext : MonoBehaviour
{
    public static SceneContext Instance { 
        get; 
        private set; 
    }
    
    public static string JwtToken
    {
        get; set;
    }
    
    public static string ServerIp = "localhost";
    public static int ServerPort = 7777;
    public static string ServerUrl = $"http://{ServerIp}:{ServerPort}";
    
    public static long UserID
    {
        get => _user.id;
    }

    private static User _user;

    public static User User
    {
        get { return _user; }
        set
        {
            Debug.Log("Setting User: " + value.name + ", ID: " + value.id);
            _user = value;
        }
    }

    public static string Me
    {
        get
        {
            if (UserID == MatchInfo.leftUser.id)
                return "LeftPlayer";
            else if (UserID == MatchInfo.rightUser.id)
                return "RightPlayer";
            return "None";
        }
    }

    public static MatchedInfoDto MatchInfo
    {
        get; set;
    }

    public static ResultInfo MatchResult
    {
        get; set;
    }
    public static string SelectedDeck
    {
        get; set;
    }
    public static string OwnedCards
    {
        get; set;
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }
}

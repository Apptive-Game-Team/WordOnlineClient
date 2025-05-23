using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneContext : MonoBehaviour
{
    public static string UserID
    {
        get; private set;
    }

    public static MatchedInfoDto MatchInfo
    {
        get; set;
    }

    public static string MatchResult
    {
        get; set;
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
        UserID = FindObjectOfType<UserInfo>().userID;
    }
}

using System.Collections;
using System.Collections.Generic;
using Script.Global;
using TMPro;
using UnityEngine;

public class LobbyUserNameUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI userNameText;

    public void SetUserName(string userName)
    {
        WDebug.Log($"Name : {userName}");
        this.userNameText.SetText($"Name : {userName} ");
        WDebug.Log("Set UserName");
    }
}

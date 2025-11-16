using System;
using System.Collections;
using System.Collections.Generic;
using Script.Global;
using Script.LobbyScene.Button;
using UnityEngine;

public class PracticeButton : LobbyButtonBase
{
    protected override void OnClickButton()
    {
        if (!isActive)
        {
            return;
        }
        
        try
        {
            StompConnector.Instance.StartPracticeFlow();
            SetButton();
        }
        catch (Exception)
        {
            ResetButton();
        }
    }
}

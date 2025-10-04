using System;
using System.Collections;
using System.Collections.Generic;
using Script.Global;
using UnityEngine;

public class PracticeButton : AsyncButtonBase
{
    protected override void OnClickButton()
    {
        try
        {
            StompConnector.Instance.StartPracticeFlow();
        }
        catch (Exception)
        {
            ResetButton();
        }
    }
}

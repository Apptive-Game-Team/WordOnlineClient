using System;
using System.Collections;
using System.Collections.Generic;
using Script.Global;
using UnityEngine;

public class EnqueueButton : AsyncButtonBase
{
    protected override void OnClickButton()
    {
        try
        {
            StompConnector.Instance.StartMatchingFlow();
        }
        catch (Exception)
        {
            ResetButton();
        }
    }
}

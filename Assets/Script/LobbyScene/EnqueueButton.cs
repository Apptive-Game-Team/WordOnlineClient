using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnqueueButton : ButtonBase
{
    protected override void OnClickButton()
    {
        StompConnector.Instance.StartMatchingFlow();
    }
}

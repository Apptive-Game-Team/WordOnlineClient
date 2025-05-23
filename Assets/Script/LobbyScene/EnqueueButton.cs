using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnqueueButton : MonoBehaviour
{
    [SerializeField] StompConnector StompConnector;

    private StompConnector GetStompConnector()
    {
        if (StompConnector == null)
        {
            StompConnector = FindObjectOfType<StompConnector>();
        }
        return StompConnector;
    }
    
    public void OnClickButton()
    {
        GetStompConnector().ConnectToServer("ws://localhost:8080/ws");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoLobbyButton : MonoBehaviour
{
    public void OnClick()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}

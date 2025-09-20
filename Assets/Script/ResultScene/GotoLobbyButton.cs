using UnityEngine.SceneManagement;

public class GotoLobbyButton : ButtonBase
{
    protected override void OnClickButton()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}

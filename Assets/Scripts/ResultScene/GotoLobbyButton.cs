using Scripts.Global.Button;
using UnityEngine.SceneManagement;

namespace Scripts.ResultScene
{
    public class GotoLobbyButton : ButtonBase
    {
        protected override void OnClickButton()
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }
}

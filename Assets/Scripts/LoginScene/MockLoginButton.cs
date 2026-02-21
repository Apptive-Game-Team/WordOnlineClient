using Scripts.Global.Button;
using UnityEngine.SceneManagement;

namespace Scripts.LoginScene
{
    public class MockLoginButton : ButtonBase
    {
        protected override void OnClickButton()
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }
}

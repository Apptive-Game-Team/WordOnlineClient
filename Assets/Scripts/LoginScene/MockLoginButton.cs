using Global.Button;
using UnityEngine.SceneManagement;

namespace LoginScene
{
    public class MockLoginButton : ButtonBase
    {
        protected override void OnClickButton()
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }
}

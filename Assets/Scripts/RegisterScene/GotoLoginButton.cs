using Global.Button;
using UnityEngine.SceneManagement;

namespace RegisterScene
{
    public class GotoLoginButton : ButtonBase
    {
        protected override void OnClickButton()
        {
            SceneManager.LoadScene("LoginScene");
        }
    }
}

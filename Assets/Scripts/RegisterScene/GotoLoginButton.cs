using Scripts.Global.Button;
using UnityEngine.SceneManagement;

namespace Scripts.RegisterScene
{
    public class GotoLoginButton : ButtonBase
    {
        protected override void OnClickButton()
        {
            SceneManager.LoadScene("LoginScene");
        }
    }
}

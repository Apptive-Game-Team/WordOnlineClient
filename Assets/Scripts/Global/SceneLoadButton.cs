using Global.Button;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Global
{
    public class SceneLoadButton : ButtonBase
    {
        [SerializeField] string sceneName;

        protected override void OnClickButton()
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                WDebug.LogError("Scene name is not set.");
                return;
            }
            SceneManager.LoadScene(sceneName);
        }
    }
}

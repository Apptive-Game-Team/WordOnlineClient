using Global.Button;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LobbyScene.Button
{
    public class LobbySceneLoadButton : ButtonBase
    {
        [SerializeField] protected string sceneName;
    
        protected override void OnClickButton()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}

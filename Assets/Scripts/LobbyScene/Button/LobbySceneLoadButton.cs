using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.LobbyScene.Button
{
    public class LobbySceneLoadButton : LobbyButtonBase
    {
        [SerializeField] protected string sceneName;
    
        protected override void OnClickButton()
        {
            if (!isActive)
            {
                return;
            }
            SceneManager.LoadScene(sceneName);
        }
    }
}

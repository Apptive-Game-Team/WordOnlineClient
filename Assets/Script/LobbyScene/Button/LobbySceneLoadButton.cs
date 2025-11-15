using Script.LobbyScene.Button;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneLoadButton : LobbyButtonBase
{
    [SerializeField] private string sceneName;
    
    protected override void OnClickButton()
    {
        if (!isActive)
        {
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}

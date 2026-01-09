using Script.LobbyScene.Button;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextChangeSceneLoadButton : LobbySceneLoadButton
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] string script;
    protected override void OnClickButton()
    {
        text.text = script;
        Invoke(nameof(LoadScene), 3f);
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}

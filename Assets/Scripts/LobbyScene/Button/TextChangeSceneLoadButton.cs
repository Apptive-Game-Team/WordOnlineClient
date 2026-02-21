using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

namespace Scripts.LobbyScene.Button
{
    public class TextChangeSceneLoadButton : LobbySceneLoadButton
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] LocalizedString message;
        protected override void OnClickButton()
        {
            text.text = message.GetLocalizedString();
            Invoke(nameof(LoadScene), 3f);
        }

        private void LoadScene()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}

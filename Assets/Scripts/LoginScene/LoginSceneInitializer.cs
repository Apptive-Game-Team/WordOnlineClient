using System.Collections;
using Global;
using TMPro;
using UnityEngine;

namespace LoginScene
{
    public class LoginSceneInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject penal;
        [SerializeField] private TMP_Text messageText;
        
        private IEnumerator Start()
        {
            using LoadingHandle loadingHandle = LoadingPage.Begin(this);

            yield return DeployStatusChecker.CheckDeployStatus((isHealthy, message) =>
            {
                if (!isHealthy)
                {
                    penal.SetActive(true);
                    messageText.text = message;
                }
            });
        }
    }
}

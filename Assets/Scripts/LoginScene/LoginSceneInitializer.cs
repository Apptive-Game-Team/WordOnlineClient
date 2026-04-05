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
            LoadingPage.Instance.IsLoading = true;

            yield return DeployStatusChecker.CheckDeployStatus((isHealthy, message) =>
            {
                LoadingPage.Instance.IsLoading = false;
                if (!isHealthy)
                {
                    penal.SetActive(true);
                    messageText.text = message;
                }
            });
        }
    }
}

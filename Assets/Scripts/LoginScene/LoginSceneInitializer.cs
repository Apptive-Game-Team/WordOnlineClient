using System.Collections;
using Global;
using UnityEngine;
using UnityEngine.UI;

namespace LoginScene
{
    public class LoginSceneInitializer : MonoBehaviour
    {
        [SerializeField] private Selectable[] interactableElements;

        private IEnumerator Start()
        {
            SetInteractable(false);
            LoadingPage.Instance.IsLoading = true;

            yield return DeployStatusChecker.CheckDeployStatus(isHealthy =>
            {
                LoadingPage.Instance.IsLoading = false;
                SetInteractable(isHealthy);
            });
        }

        private void SetInteractable(bool interactable)
        {
            foreach (var element in interactableElements)
            {
                if (element != null)
                    element.interactable = interactable;
            }
        }
    }
}

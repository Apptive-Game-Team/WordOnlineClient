using System.Collections;
using Script.Global;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Script.LobbyScene.SettingPage
{
    public class DeleteAccountButton : AsyncButtonBase
    {
        protected override void OnClickButton()
        {
            StartCoroutine(DeleteAccount());
        }

        private IEnumerator DeleteAccount()
        {
            using (UnityWebRequest request = UnityWebRequest.Delete($"{SceneContext.CurrentServer.url}/api/users/mine"))
            {
                request.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
                
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Account deleted successfully.");
                    SceneContext.ClearContext();
                    SystemMessageUI.Instance.ShowMessage("성공적으로 탈퇴되었습니다.\n초기 화면으로 이동합니다.");
                    yield return new WaitForSeconds(2f);
                    SceneManager.LoadScene("LoginScene");
                    yield break;
                }
                Debug.LogError($"Error deleting account: {request.error}");
                ResetButton();
            }
            ResetButton();
        }
    }
}
using System.Collections;
using Script.Data;
using Script.Global;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Script.LobbyScene.SettingPage
{
    public class DeleteAccountButton : AsyncButtonBase
    {
        
        public LocalizedString confirmationMessage;
        
        protected override void OnClickButton()
        {
            StartCoroutine(DeleteAccount());
        }

        private IEnumerator DeleteAccount()
        {
            using UnityWebRequest requestToServer = UnityWebRequest.Delete($"{ServerList.MatchingServer.url}/api/users/mine");
            using UnityWebRequest requestToAccount = UnityWebRequest.Delete($"{ServerList.AccountServer.url}/api/members/me");
            Server.SetAcceptLanguage(requestToServer);
            Server.SetAcceptLanguage(requestToAccount);
            Server.SetAuthorization(requestToServer);
            Server.SetAuthorization(requestToAccount);
            
            yield return requestToServer.SendWebRequest();
            yield return requestToAccount.SendWebRequest();

            if (requestToServer.result == UnityWebRequest.Result.Success && requestToAccount.result == UnityWebRequest.Result.Success)
            {
                WDebug.Log("Account deleted successfully.");
                SceneContext.ClearContext();
                SystemMessageUI.Instance.ShowMessage(confirmationMessage);
                yield return new WaitForSeconds(2f);
                SceneManager.LoadScene("LoginScene");
                yield break;
            }
            WDebug.LogError($"Error deleting account: {requestToServer.error}");
            ResetButton();
        }
    }
}
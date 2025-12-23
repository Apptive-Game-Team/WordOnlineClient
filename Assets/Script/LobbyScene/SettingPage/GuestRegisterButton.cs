using System.Collections;
using Script.Data;
using Script.Global;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Script.LobbyScene.SettingPage
{

    public class GuestRegisterButton : AsyncButtonBase
    {
        [SerializeField] private InputField nameInputField;
        [SerializeField] private InputField emailInputField;
        [SerializeField] private InputField passwordInputField;
        
        private IEnumerator RegisterCoroutine(MemberPutRequest memberPutRequest)
        {
            string jsonData = JsonUtility.ToJson(memberPutRequest);

            using UnityWebRequest webRequest = new UnityWebRequest(ServerList.AccountServer.url + "/api/members/me", "PUT");
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError("Error: " + webRequest.downloadHandler.text);
                SystemMessageUI.Instance.ShowMessage(webRequest.downloadHandler.text);
                ResetButton();
                yield break;
            }
                
            WDebug.Log("Response: " + webRequest.downloadHandler.text);
            
            SceneContext.ClearContext();
            
            SceneManager.LoadScene("LoginScene");
        }

        protected override void OnClickButton()
        {
            StartCoroutine(RegisterCoroutine(
                new MemberPutRequest(nameInputField.text, emailInputField.text, passwordInputField.text, GuestContext.GuestPassword)
            ));
        }
    }
}
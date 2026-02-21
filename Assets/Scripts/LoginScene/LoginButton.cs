using System.Collections;
using Scripts.Data;
using Scripts.Global;
using Scripts.Global.Button;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scripts.LoginScene
{
    public class LoginButton : AsyncButtonBase
    {
        [SerializeField] private InputField emailInputField;
        [SerializeField] private InputField passwordInputField;

        public LocalizedString loginErrorProcessingResponse;
    
        private IEnumerator LoginCoroutine(LoginRequestDto loginRequestDto)
        {
            string jsonData = JsonUtility.ToJson(loginRequestDto);
        
            using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.AccountServer.url + "/api/members/login", "POST"))
            {
                Server.SetAcceptLanguage(webRequest);
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.timeout = 10;
            
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    WDebug.LogError("Error: " + webRequest.error);
                    SystemMessageUI.Instance.ShowMessage(loginErrorProcessingResponse);
                    ResetButton();
                    yield break;
                }
            
                WDebug.Log("Response: " + webRequest.downloadHandler.text);

                try
                {
                    AuthResponseDto authResponseDto = JsonUtility.FromJson<AuthResponseDto>(webRequest.downloadHandler.text);
            
                    SceneContext.JwtToken = authResponseDto.jwt;
            
                    SceneManager.LoadScene("LobbyScene");
                } catch (System.Exception e)
                {
                    WDebug.LogError("Parsing Error: " + e.Message);
                    SystemMessageUI.Instance.ShowMessage(loginErrorProcessingResponse);
                    ResetButton();
                }
            }
        }

        protected override void OnClickButton()
        {
            StartCoroutine(LoginCoroutine(
                new LoginRequestDto(emailInputField.text, passwordInputField.text)
            ));
        }
    }
}

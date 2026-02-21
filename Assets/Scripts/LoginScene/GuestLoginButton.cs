using System.Collections;
using Scripts.Data;
using Scripts.Global;
using Scripts.Global.Button;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Scripts.LoginScene
{
    public class GuestLoginButton : AsyncButtonBase
    {
    
        private IEnumerator GuestLoginCoroutine()
        {
        
            using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.AccountServer.url + "/api/members/guest", "POST"))
            {
                Server.SetAcceptLanguage(webRequest);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.timeout = 10;
                webRequest.SetRequestHeader("Content-Type", "application/json");
            
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    WDebug.LogError("Error: " + webRequest.error);
                    SystemMessageUI.Instance.ShowMessage(webRequest.downloadHandler.text);
                    ResetButton();
                    yield break;
                }
            
                WDebug.Log("Response: " + webRequest.downloadHandler.text);
            
                GuestAuthResponseDto authResponseDto = JsonUtility.FromJson<GuestAuthResponseDto>(webRequest.downloadHandler.text);
            
                SceneContext.JwtToken = authResponseDto.jwt;
                GuestContext.GuestPassword = authResponseDto.password;
            
                SceneManager.LoadScene("TutorialScene");
            }
        }

        protected override void OnClickButton()
        {
            StartCoroutine(GuestLoginCoroutine());
        }
    }
}

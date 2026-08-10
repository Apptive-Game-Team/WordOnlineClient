using System.Collections;
using Data;
using Global;
using Global.Button;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Global.Serialization;

namespace LoginScene
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
            
                GuestAuthResponseDto authResponseDto = JsonCodec.Deserialize<GuestAuthResponseDto>(webRequest.downloadHandler.text);
            
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

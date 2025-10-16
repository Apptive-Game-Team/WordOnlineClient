
using System.Collections;
using Script.Data;
using Script.Global;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GuestLoginButton : AsyncButtonBase
{
    private IEnumerator GuestLoginCoroutine()
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.AccountServer.url + "/api/members/guest", "POST"))
        {
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = 10;
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                SystemMessageUI.Instance.ShowMessage(webRequest.downloadHandler.text);
                ResetButton();
                yield break;
            }
            
            Debug.Log("Response: " + webRequest.downloadHandler.text);
            
            AuthResponseDto authResponseDto = JsonUtility.FromJson<AuthResponseDto>(webRequest.downloadHandler.text);
            
            SceneContext.JwtToken = authResponseDto.jwt;
            
            SceneManager.LoadScene("TutorialScene");
        }
    }

    protected override void OnClickButton()
    {
        StartCoroutine(GuestLoginCoroutine());
    }
}

using System.Collections;
using System.Collections.Generic;
using Script.Data;
using Script.Global;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GuestLoginButton : AsyncButtonBase
{
    
    [SerializeField] private InputField userName;
    
    private IEnumerator GuestLoginCoroutine()
    {
        Dictionary<string, string> formData = new Dictionary<string, string>()
        {
            {"name", userName.text}
        };
        
        using (UnityWebRequest webRequest = UnityWebRequest.Post(ServerList.AccountServer.url + "/api/members/guest", formData))
        {
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = 10;
            
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
            
            SceneManager.LoadScene("LobbyScene");
        }
    }

    protected override void OnClickButton()
    {
        StartCoroutine(GuestLoginCoroutine());
    }
}

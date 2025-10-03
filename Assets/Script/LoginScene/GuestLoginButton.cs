using System;
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
    
    [Serializable]
    private class GuestRequestDto
    {
        public string name;
        
        public GuestRequestDto(string name)
        {
            this.name = name;
        }
    }
    
    private IEnumerator GuestLoginCoroutine()
    {
        Debug.Log("GuestLoginCoroutine Start UserName: " + userName.text);
        
        using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.AccountServer.url + "/api/members/guest", "POST"))
        {
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = 10;
            webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(
                JsonUtility.ToJson(new GuestRequestDto(userName.text))
            ));
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            yield return webRequest.SendWebRequest();

            Debug.Log("Request Headers:"); 
            Debug.Log(webRequest.GetRequestHeader("Content-Type"));
            

            Debug.Log("Request Upload Content:");
            Debug.Log(System.Text.Encoding.UTF8.GetString(webRequest.uploadHandler.data));
            
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

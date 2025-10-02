using System.Collections;
using System.Collections.Generic;
using Script.Data;
using Script.Global;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginButton : AsyncButtonBase
{
    [SerializeField] private InputField emailInputField;
    [SerializeField] private InputField passwordInputField;
    
    private IEnumerator LoginCoroutine(LoginRequestDto loginRequestDto)
    {
        string jsonData = JsonUtility.ToJson(loginRequestDto);
        
        using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.AccountServer.url + "/api/members/login", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
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
        StartCoroutine(LoginCoroutine(
            new LoginRequestDto(emailInputField.text, passwordInputField.text)
        ));
    }
}

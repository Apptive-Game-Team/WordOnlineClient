using System.Collections;
using Script.Data;
using Script.Global;
using Script.RegisterScene;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RegisterButton : AsyncButtonBase
{
    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField emailInputField;
    [SerializeField] private InputField passwordInputField;
    
    private IEnumerator RegisterCoroutine(RegisterRequestDto registerRequestDto)
    {
        string jsonData = JsonUtility.ToJson(registerRequestDto);
        
        using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.AccountServer.url + "/api/members", "POST"))
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
                WDebug.LogError("Error: " + webRequest.downloadHandler.text);
                SystemMessageUI.Instance.ShowMessage(webRequest.downloadHandler.text);
                ResetButton();
                yield break;
            }
            
            WDebug.Log("Response: " + webRequest.downloadHandler.text);
            
            AuthResponseDto authResponseDto = JsonUtility.FromJson<AuthResponseDto>(webRequest.downloadHandler.text);
            
            SceneContext.JwtToken = authResponseDto.jwt;
                    
            SceneManager.LoadScene("TutorialScene");
        }
    }

    protected override void OnClickButton()
    {
        StartCoroutine(RegisterCoroutine(
            new RegisterRequestDto(emailInputField.text, passwordInputField.text, nameInputField.text)
        ));
    }
}

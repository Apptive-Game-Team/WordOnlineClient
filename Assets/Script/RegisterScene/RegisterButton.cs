using System.Collections;
using Script.Global;
using Script.RegisterScene;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class RegisterButton : AsyncButtonBase
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    
    private IEnumerator RegisterCoroutine(RegisterRequestDto registerRequestDto)
    {
        string jsonData = JsonUtility.ToJson(registerRequestDto);
        
        using (UnityWebRequest webRequest = new UnityWebRequest(SceneContext.CurrentServer.url + "/api/users", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
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
            
            SceneContext.JwtToken = authResponseDto.jwtToken;
        }
        
        SceneManager.LoadScene("LobbyScene");
    }

    protected override void OnClickButton()
    {
        StartCoroutine(RegisterCoroutine(
            new RegisterRequestDto(emailInputField.text, passwordInputField.text, nameInputField.text)
        ));
    }
}


using System.Collections;
using Script.Data;
using UnityEngine;
using UnityEngine.Networking;

public class UserInfoGetter
{
    public static IEnumerator GetUserInfo()
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(SceneContext.CurrentServer.url + "/api/users/mine", "GET"))
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                yield break;
            }
            
            SceneContext.User = JsonUtility.FromJson<User>(webRequest.downloadHandler.text);
        }
    }
    
    public static IEnumerator GetUserStatus()
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(SceneContext.ServerUrl + "/api/users/mine", "GET"))
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
                yield break;
            }
            
            SceneContext.User = JsonUtility.FromJson<User>(webRequest.downloadHandler.text);
        }
    }
}

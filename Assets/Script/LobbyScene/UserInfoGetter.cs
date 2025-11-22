
using System.Collections;
using Script.Data;
using Script.Global;
using UnityEngine;
using UnityEngine.Networking;

public class UserInfoGetter
{
    public static IEnumerator GetUserInfo()
    {
        AccountUser accountUser;
        GameUser gameUser;
        using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.AccountServer.url + "/api/members/me", "GET"))
        {
            Server.SetAcceptLanguage(webRequest);
            webRequest.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError("Error: " + webRequest.error);
                yield break;
            }
            
            accountUser =JsonUtility.FromJson<AccountUser>(webRequest.downloadHandler.text);
        }
        
        using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.MatchingServer.url + "/api/users/mine", "GET"))
        {
            Server.SetAcceptLanguage(webRequest);
            webRequest.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError("Error: " + webRequest.error);
                yield break;
            }
            
            gameUser = JsonUtility.FromJson<GameUser>(webRequest.downloadHandler.text);
        }

        SceneContext.User = new User(accountUser, gameUser);
    }
    
    
    public static IEnumerator GetUserStatus()
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.MatchingServer.url + "/api/users/mine", "GET"))
        {
            Server.SetAcceptLanguage(webRequest);
            webRequest.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError("Error: " + webRequest.error);
                yield break;
            }
            
            SceneContext.User = JsonUtility.FromJson<User>(webRequest.downloadHandler.text);
        }
    }
}

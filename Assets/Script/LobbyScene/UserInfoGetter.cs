
using System.Collections;
using Script.Data;
using Script.Global;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class UserInfoGetter
{
    public static IEnumerator GetUserInfo()
    {
        AccountUser accountUser;
        GameUser gameUser;
        using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.AccountServer.url + "/api/members/me", "GET"))
        {
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError("Error: " + webRequest.error);
                SystemMessageUI.Instance.ShowMessage("Failed to retrieve user data. Please log in again.");
                LoadingPage.Instance.IsLoading = false;
                SceneManager.LoadScene("LoginScene");
                yield break;
            }
            
            accountUser = JsonUtility.FromJson<AccountUser>(webRequest.downloadHandler.text);
        }
        
        using (UnityWebRequest webRequest = new UnityWebRequest(ServerList.MatchingServer.url + "/api/users/mine", "GET"))
        {
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError("Error: " + webRequest.error);
                SystemMessageUI.Instance.ShowMessage("Failed to retrieve user data. Please log in again.");
                LoadingPage.Instance.IsLoading = false;
                SceneManager.LoadScene("LoginScene");
                yield break;
            }
            
            gameUser = JsonUtility.FromJson<GameUser>(webRequest.downloadHandler.text);
        }

        SceneContext.User = new User(accountUser, gameUser);
    }
}

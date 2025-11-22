using System;
using System.Collections;
using Script.Data;
using Script.Global;
using Script.LobbyScene;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public static class StatusTracker
{
    [Serializable]
    private class StatusDto
    {
        public string status;
    } 

    [Serializable]
    public class SessionDto
    {
        public string sessionId;
        public User leftUser;
        public User rightUser;
    }

    private static LocalizedString sessionRestoredMatching = new LocalizedString { TableReference = "SystemMessageUI", TableEntryReference = "sessionRestoredMatching" };
    private static LocalizedString sessionRestoredGame = new LocalizedString { TableReference = "SystemMessageUI", TableEntryReference = "sessionRestoredGame" };

    public static IEnumerator GetUserStatus()
    {
        var url = ServerList.MatchingServer.url + "/api/users/mine/status";

        using var www = UnityWebRequest.Get(url);
        Server.SetAcceptLanguage(www);
        Server.SetAuthorization(www);
        
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            WDebug.LogError($"[GetUserStatus] fail: {www.responseCode} / {www.error}");
            yield break;
        }

        StatusDto dto = null;
        try { dto = JsonUtility.FromJson<StatusDto>(www.downloadHandler.text); }
        catch (Exception e)
        {
            WDebug.LogError($"[GetUserStatus] JSON parse error: {e}\n{www.downloadHandler.text}");
            yield break;
        }

        if (dto == null || string.IsNullOrEmpty(dto.status))
        {
            WDebug.LogWarning("[GetUserStatus] empty status");
            yield break;
        }

        yield return HandleUserStatus(dto.status);
    }

    private static IEnumerator HandleUserStatus(string status)
    {
        switch (status)
        {
            case "Online":
                yield break;

            case "OnMatching":
                SystemMessageUI.Instance.ShowMessage(sessionRestoredMatching);
                StompConnector.Instance.ConnectToServer();
                LobbySceneViewModel.Instance.Enqueue();
                yield break;

            case "OnPlaying":
                yield return RecoverGameSession();
                yield break;
            default:
                WDebug.LogWarning($"[GetUserStatus] unknown status: {status}");
                yield break;
        }
    }
    
    private static IEnumerator RecoverGameSession()
    {
        var getSessionUrl = $"{ServerList.MatchingServer.url}/api/users/mine/match-info";
                
        var getSessionReq = UnityWebRequest.Get(getSessionUrl);
        Server.SetAcceptLanguage(getSessionReq);
        Server.SetAuthorization(getSessionReq);
                
        yield return getSessionReq.SendWebRequest();

        if (getSessionReq.result != UnityWebRequest.Result.Success)
        {
            WDebug.LogError($"[EnterInGameByMine] fail: {getSessionReq.responseCode} / {getSessionReq.error}\n{getSessionReq.downloadHandler.text}");
            yield break;
        }

        if (getSessionReq.responseCode == 404)
        {
            WDebug.Log("[EnterInGameByMine] NO_SESSION (로비/매칭 상태)");
            yield break;
        }

        string json = getSessionReq.downloadHandler.text;
        MatchedInfoDto matchedInfoDto = JsonUtility.FromJson<MatchedInfoDto>(json);
        SceneContext.MatchInfo = matchedInfoDto;
        SceneManager.LoadScene("GameScene");
    }
}

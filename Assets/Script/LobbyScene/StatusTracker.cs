using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public static class StatusTracker
{
    [Serializable]
    private class StatusDto
    {
        public string status;
    } 

    public static IEnumerator GetUserStatus()
    {
        var url = SceneContext.CurrentServer.url + "/api/users/mine/status";

        using var www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[GetUserStatus] fail: {www.responseCode} / {www.error}");
            yield break;
        }

        StatusDto dto = null;
        try { dto = JsonUtility.FromJson<StatusDto>(www.downloadHandler.text); }
        catch (Exception e)
        {
            Debug.LogError($"[GetUserStatus] JSON parse error: {e}\n{www.downloadHandler.text}");
            yield break;
        }

        if (dto == null || string.IsNullOrEmpty(dto.status))
        {
            Debug.LogWarning("[GetUserStatus] empty status");
            yield break;
        }

        switch (dto.status)
        {
            case "Online":
                yield break;

            case "OnMatching":
                StompConnector.Instance.ConnectToServer($"{SceneContext.CurrentServer.webSocketUrl}+ {SceneContext.JwtToken}");
                StompConnector.Instance.StartReMatchingFlow();
                yield break;

            case "OnPlaying":
                StompConnector.Instance.ConnectToServer($"{SceneContext.CurrentServer.webSocketUrl}+ {SceneContext.JwtToken}");
                // StompConnector.Instance.StartInGameFlow();
                SceneManager.LoadScene("GameScene"); 
                
                var snapUrl = $"{SceneContext.CurrentServer.url}/sessions/{SceneContext.MatchInfo.sessionId}/snapshot";
                using (var snapReq = UnityWebRequest.Get(snapUrl))
                {
                    snapReq.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
                    yield return snapReq.SendWebRequest();

                    if (snapReq.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[GetUserStatus] snapshot fail: {snapReq.responseCode} / {snapReq.error}\n{snapReq.downloadHandler.text}");
                        yield break;
                    }
                    //
                    // // 네 쪽 렌더러/팩토리에 그대로 넘겨서 생성/갱신
                    Debug.Log(snapReq.downloadHandler.text);
                    // ApplySnapshotJson(snapReq.downloadHandler.text);
                }
                yield break;

            default:
                Debug.LogWarning($"[GetUserStatus] unknown status: {dto.status}");
                yield break;
        }
    }


    private static void ApplySnapshotJson(string json)
    {
        
    }
}

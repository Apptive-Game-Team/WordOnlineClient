using System;
using System.Collections;
using Script.Data;
using Script.Global;
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

    [Serializable]
    public class SessionDto
    {
        public string sessionId;
        public User leftUser;
        public User rightUser;
    }
    
    public static IEnumerator GetUserStatus()
    {
        var url = SceneContext.CurrentServer.url + "/api/users/mine/status";

        using var www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
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

        switch (dto.status)
        {
            case "Online":
                yield break;

            case "OnMatching":
                SystemMessageUI.Instance.ShowMessage("세션이 복구되어 매칭 탐색을 재개했습니다.");
                StompConnector.Instance.ConnectToServer();
                try
                {
                    StompConnector.Instance.StartMatchingFlow();
                }
                catch (Exception)
                {
                    GameObject.FindObjectOfType<EnqueueButton>().ResetButton();
                }
                yield break;

            case "OnPlaying":
                StompConnector.Instance.ConnectToServer();  
                
                var getSessionUrl = $"{SceneContext.CurrentServer.url}/sessions/mine";
                
                var getSessionReq = UnityWebRequest.Get(getSessionUrl);
                getSessionReq.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
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

                // 200 OK
                SessionDto sessionDto = null;
                try { sessionDto = JsonUtility.FromJson<SessionDto>(getSessionReq.downloadHandler.text); }
                catch (Exception e)
                {
                    WDebug.LogError($"[EnterInGameByMine] JSON parse error: {e}\n{getSessionReq.downloadHandler.text}");
                    yield break;
                }
                if (sessionDto == null || string.IsNullOrEmpty(sessionDto.sessionId))
                {
                    WDebug.LogError("[EnterInGameByMine] sessionId empty");
                    yield break;
                }
                SystemMessageUI.Instance.ShowMessage("세션이 복구되어 진행 중인 게임에 연결했습니다.");
                // 컨텍스트에 저장(선택)
                if (SceneContext.MatchInfo == null) SceneContext.MatchInfo = new MatchedInfoDto();
                SceneContext.MatchInfo.sessionId = sessionDto.sessionId;
                SceneContext.MatchInfo.rightUser = sessionDto.rightUser;
                SceneContext.MatchInfo.leftUser = sessionDto.leftUser;
                
                var snapUrl = $"{SceneContext.CurrentServer.url}/sessions/{SceneContext.MatchInfo.sessionId}/snapshot";
                using (var snapReq = UnityWebRequest.Get(snapUrl))
                {
                    snapReq.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
                    yield return snapReq.SendWebRequest();
                    if (snapReq.result != UnityWebRequest.Result.Success)
                    {
                        WDebug.LogError($"[GetUserStatus] snapshot fail: {snapReq.responseCode} / {snapReq.error}\n{snapReq.downloadHandler.text}");
                        yield break;
                    }
                    //
                    // // 네 쪽 렌더러/팩토리에 그대로 넘겨서 생성/갱신
                    WDebug.Log($"[snapshot] snapshot:{snapReq.downloadHandler.text}");
                    SceneContext.RejoinSyncJson = snapReq.downloadHandler.text;
                    
                    // 인게임 플로우로 전환
                    StompConnector.Instance.StartInGameFlow(sessionDto.sessionId);
                    SceneManager.LoadScene("GameScene"); 
                }
                yield break;

            default:
                WDebug.LogWarning($"[GetUserStatus] unknown status: {dto.status}");
                yield break;
            
            
        }
    }
}

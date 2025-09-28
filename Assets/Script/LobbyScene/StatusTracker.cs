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

    [Serializable]
    public class SessionIdDto
    {
        public string sessionId;
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
                SystemMessageUI.Instance.ShowMessage("세션이 복구되어 매칭 탐색을 재개했습니다.");
                GameObject.FindObjectOfType<EnqueueButton>().ButtonEvent();
                StompConnector.Instance.ConnectToServer();
                StompConnector.Instance.StartMatchingFlow();
                yield break;

            case "OnPlaying":
                StompConnector.Instance.ConnectToServer();  
                
                var getIDUrl = $"{SceneContext.CurrentServer.url}/sessions/mine";
                
                var getIDReq = UnityWebRequest.Get(getIDUrl);
                getIDReq.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
                yield return getIDReq.SendWebRequest();

                if (getIDReq.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[EnterInGameByMine] fail: {getIDReq.responseCode} / {getIDReq.error}\n{getIDReq.downloadHandler.text}");
                    yield break;
                }

                if (getIDReq.responseCode == 404)
                {
                    Debug.Log("[EnterInGameByMine] NO_SESSION (로비/매칭 상태)");
                    yield break;
                }

                // 200 OK
                SessionIdDto sessionDto = null;
                try { sessionDto = JsonUtility.FromJson<SessionIdDto>(getIDReq.downloadHandler.text); }
                catch (Exception e)
                {
                    Debug.LogError($"[EnterInGameByMine] JSON parse error: {e}\n{getIDReq.downloadHandler.text}");
                    yield break;
                }
                if (sessionDto == null || string.IsNullOrEmpty(sessionDto.sessionId))
                {
                    Debug.LogError("[EnterInGameByMine] sessionId empty");
                    yield break;
                }
                SystemMessageUI.Instance.ShowMessage("세션이 복구되어 진행 중인 게임에 연결했습니다.");
                // 컨텍스트에 저장(선택)
                if (SceneContext.MatchInfo == null) SceneContext.MatchInfo = new MatchedInfoDto();
                SceneContext.MatchInfo.sessionId = sessionDto.sessionId;
                SceneContext.

                // 인게임 플로우로 전환
                StompConnector.Instance.StartInGameFlow(sessionDto.sessionId);
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

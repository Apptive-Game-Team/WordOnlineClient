using System;
using System.Collections;
using Data;
using Global;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace LobbyScene
{
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

        private static readonly LocalizedString sessionRestoredMatching = new LocalizedString { TableReference = "SystemMessageUI", TableEntryReference = "sessionRestoredMatching" };
        private static LocalizedString sessionRestoredGame = new LocalizedString { TableReference = "SystemMessageUI", TableEntryReference = "sessionRestoredGame" };

        public static IEnumerator GetUserStatus()
        {
            return GetUserStatus(HandleUserStatus);
        }
    
        public static IEnumerator GetUserStatus(Func<string, IEnumerator> handler)
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
            bool jsonParseError = false;
            try { dto = JsonUtility.FromJson<StatusDto>(www.downloadHandler.text); }
            catch (Exception e)
            {
                WDebug.LogError($"[GetUserStatus] JSON parse error: {e}\n{www.downloadHandler.text}");
                jsonParseError = true;
            }

            if (jsonParseError || dto == null || string.IsNullOrEmpty(dto.status))
            {
                WDebug.LogWarning("[GetUserStatus] empty status");
                yield return handler.Invoke(null);
                yield break;
            }

            WDebug.Log("[GetUserStatus] successfully recover status: " + dto.status);

            yield return handler.Invoke(dto.status);
        }

        private static IEnumerator HandleUserStatus(string status)
        {
            switch (status)
            {
                case "Online":
                    yield break;

                case "OnMatching":
                    SystemMessageUI.Instance.ShowMessage(sessionRestoredMatching);
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
    
        public static IEnumerator RecoverGameSession()
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
        
            MatchedInfoDto matchedInfoDto;
            try
            {
                string json = getSessionReq.downloadHandler.text;
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    WDebug.LogError("[EnterInGameByMine] Response body is empty.");
                    yield break;
                }

                matchedInfoDto = JsonUtility.FromJson<MatchedInfoDto>(json);

                if (matchedInfoDto == null)
                {
                    WDebug.LogError($"[EnterInGameByMine] Failed to parse JSON.\n{json}");
                    yield break;
                }
            }
            catch (Exception e)
            {
                WDebug.LogError($"[EnterInGameByMine] JSON parse error: {e}\n{getSessionReq.downloadHandler.text}");
                yield break;
            }
            finally
            {
                WDebug.Log("[EnterInGameByMine] successfully recover session: " + getSessionReq.downloadHandler.text);
            }

            SceneContext.MatchInfo = matchedInfoDto;
            yield return GameDataRefresh.Refresh();
            SceneManager.LoadScene("GameScene");
        }
    }
}

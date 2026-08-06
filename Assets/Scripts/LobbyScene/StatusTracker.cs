using System;
using System.Collections;
using Data;
using Global;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Global.Serialization;

namespace LobbyScene
{
    public static class StatusTracker
    {
        [Serializable]
        public class SessionDto
        {
            public string sessionId;
            public User leftUser;
            public User rightUser;
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

                matchedInfoDto = JsonCodec.Deserialize<MatchedInfoDto>(json);

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

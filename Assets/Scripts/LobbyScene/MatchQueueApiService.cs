using System;
using System.Collections;
using Data;
using Global;
using UnityEngine;
using UnityEngine.Networking;
using Global.Serialization;

namespace LobbyScene
{
    public class MatchQueueApiService : MonoBehaviour
    {
        public IEnumerator Enqueue(Action<SimpleMessageDto> callback)
        {
            using var webRequest = UnityWebRequest.Get($"{ServerList.MatchingServer.url}/api/match/queue/me");
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                SimpleMessageDto messageDto = JsonCodec.Deserialize<SimpleMessageDto>(webRequest.downloadHandler.text);
                callback(messageDto);
            }
            else
            {
                WDebug.LogError($"Enqueue error: {webRequest.error}");
                callback(null);
            }
        }

        public IEnumerator MatchPractice(Action<MatchedInfoDto> callback)
        {
            using var webRequest = UnityWebRequest.Get($"{ServerList.MatchingServer.url}/api/match/practice/me");
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                MatchedInfoDto matchedInfoDto = JsonCodec.Deserialize<MatchedInfoDto>(webRequest.downloadHandler.text);
                callback(matchedInfoDto);
            }
            else
            {
                WDebug.LogError($"MatchPractice error: {webRequest.error}");
                callback(null);
            }
        }
        
        public IEnumerator RemoveFromQueue(Action<bool> callback = null)
        {
            using var webRequest = new UnityWebRequest($"{ServerList.MatchingServer.url}/api/match/queue/me", "DELETE");
        
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            
            yield return webRequest.SendWebRequest();

            bool isSuccess = webRequest.result == UnityWebRequest.Result.Success;
            if (!isSuccess)
            {
                WDebug.LogError($"RemoveFromQueue error: {webRequest.error}");
            }

            callback?.Invoke(isSuccess);
        }
    
        public IEnumerator IsMeInQueue(Action<LobbySceneViewModel.LobbyState> callback)
        {
            using var webRequest = new UnityWebRequest($"{ServerList.MatchingServer.url}/api/match/queue/me/exist", "GET");
        
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            
            yield return webRequest.SendWebRequest();

            if (webRequest.responseCode == 200)
            {
                WDebug.Log("User is in queue.");
                callback(LobbySceneViewModel.LobbyState.Matching);
            }
            else if (webRequest.responseCode == 404)
            {
                WDebug.Log("Not in queue.");
                callback(LobbySceneViewModel.LobbyState.Idle);
            }
            else
            {
                WDebug.LogError($"Error checking queue status: {webRequest.error}");
            }
        }

    }
}

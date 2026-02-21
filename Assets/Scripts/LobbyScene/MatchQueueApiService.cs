using System;
using System.Collections;
using Data;
using Data.Sse;
using Global;
using UnityEngine;
using UnityEngine.Networking;

namespace LobbyScene
{
    public class MatchQueueApiService : MonoBehaviour
    {
        [SerializeField] private SseHandler sseHandler;

        public IEnumerator Enqueue(Action<string> callback)
        {
            sseHandler.StartSse($"{ServerList.MatchingServer.url}/api/match/queue/me", callback);
            yield return null;
        }

        public IEnumerator MatchPractice(Action<string> callback)
        {
            sseHandler.StartSse($"{ServerList.MatchingServer.url}/api/match/practice/me", callback);
            yield return null;
        }
        
        public IEnumerator RemoveFromQueue()
        {
            using var webRequest = new UnityWebRequest($"{ServerList.MatchingServer.url}/api/match/queue/me", "DELETE");
        
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            
            yield return webRequest.SendWebRequest();
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
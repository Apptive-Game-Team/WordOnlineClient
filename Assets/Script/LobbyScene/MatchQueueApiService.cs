using System;
using System.Collections;
using System.Net;
using Script.Data;
using Script.Data.Sse;
using Script.Global;
using UnityEngine;
using UnityEngine.Networking;

namespace Script.LobbyScene
{
    public class MatchQueueApiService : MonoBehaviour
    {
        [SerializeField] private SseHandler sseHandler;
        
        public LobbySceneViewModel.LobbyState Enqueue()
        {
            try
            {
                // StompConnector.Instance.StartMatchingFlow();
                return LobbySceneViewModel.LobbyState.Matching;
            }
            catch (Exception)
            {
                WDebug.LogError($"Error starting match queue: {ServerList.MatchingServer}");
                return LobbySceneViewModel.LobbyState.Idle;
            }
        }

        public IEnumerator Enqueue(Action<string> callback)
        {
            sseHandler.StartSse($"{ServerList.MatchingServer.url}/api/match/queue/me", callback);
            // yield return SseHandler.ListenSseCoroutine();
            // var task = SseHandler.ListenSse($"{ServerList.MatchingServer.url}/api/match/queue/me", callback);

            // while (!task.IsCompleted)
            // {
            //     yield return null;
            // }
            //
            // if (task.Exception != null)
            // {
            //     Debug.LogError(task.Exception);
            // }
            yield return null;
        }

        public IEnumerator MatchPractice(Action<string> callback)
        {
            // yield return SseHandler.ListenSseCoroutine($"{ServerList.MatchingServer.url}/api/match/practice/me", callback);
            sseHandler.StartSse($"{ServerList.MatchingServer.url}/api/match/practice/me", callback);
            // while (!task.IsCompleted)
            // {
            //     yield return null;
            // }
            //
            // if (task.Exception != null)
            // {
            //     Debug.LogError(task.Exception);
            // }
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
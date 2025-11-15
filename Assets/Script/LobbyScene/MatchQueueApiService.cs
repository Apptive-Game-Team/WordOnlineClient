using System;
using System.Collections;
using Script.Global;
using UnityEngine;
using UnityEngine.Networking;

namespace Script.LobbyScene
{
    public class MatchQueueApiService : MonoBehaviour
    {
        public LobbySceneViewModel.LobbyState Enqueue()
        {
            try
            {
                StompConnector.Instance.StartMatchingFlow();
                return LobbySceneViewModel.LobbyState.Matching;
            }
            catch (Exception)
            {
                WDebug.LogError($"Error starting match queue: {SceneContext.CurrentServer}");
                return LobbySceneViewModel.LobbyState.Idle;
            }
        }
        
        public IEnumerator RemoveFromQueue()
        {
            using var webRequest = UnityWebRequest.Delete($"{SceneContext.CurrentServer}/api/match/queue/me");
        
            yield return webRequest.SendWebRequest();
        }
    
        public IEnumerator IsMeInQueue(Action<LobbySceneViewModel.LobbyState> callback)
        {
            using var webRequest = UnityWebRequest.Get($"{SceneContext.CurrentServer}/api/match/queue/me");
        
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
using System;
using System.Collections;
using Data;
using Data.Sse;
using UnityEngine;
using UnityEngine.Networking;

namespace Adventures
{
    public class AdventureApiService : MonoBehaviour
    {
        private const string DebugBaseUrl = "http://localhost:7777";

        [SerializeField] private SseHandler sseHandler;

        public IEnumerator RequestPVE(long scenarioId, Action<string> callback)
        {
            sseHandler.StartSse($"{ServerList.MatchingServer.url}/api/scenarios/{scenarioId}/play", callback);
            yield return null;
        }

        public IEnumerator RequestDebugPVE(long scenarioId, Action<string> onSuccess, Action<string> onFailure)
        {
            using var request = UnityWebRequest.PostWwwForm(
                $"{DebugBaseUrl}/api/debug/game/pve/{scenarioId}", "");

            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = string.IsNullOrWhiteSpace(request.downloadHandler.text)
                    ? request.error
                    : request.downloadHandler.text;
                onFailure?.Invoke(errorMessage);
                yield break;
            }

            onSuccess?.Invoke(request.downloadHandler.text);
        }
    }
}

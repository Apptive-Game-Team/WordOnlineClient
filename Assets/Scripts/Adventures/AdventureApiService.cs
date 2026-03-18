using System;
using System.Collections;
using Data;
using Data.Sse;
using UnityEngine;

namespace Adventures
{
    public class AdventureApiService : MonoBehaviour
    {
        [SerializeField] private SseHandler sseHandler;

        public IEnumerator RequestPVE(long scenarioId, Action<string> callback)
        {
            sseHandler.StartSse($"{ServerList.MatchingServer.url}/api/scenarios/{scenarioId}/play", callback);
            yield return null;
        }
    }
}
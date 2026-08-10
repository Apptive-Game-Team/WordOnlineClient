using System;
using System.Collections;
using Global;
using UnityEngine;
using UnityEngine.Networking;
using Global.Serialization;

namespace Data.Versioning
{
    public abstract class VersionedApiClient<TResponse>
    {
        protected abstract string Endpoint { get; }
        public string SourceUrl => Endpoint;
        protected virtual void OnSuccessRawJson(string json) { }

        public IEnumerator Get(Action<TResponse> onSuccess, string currentVersion = null)
        {
            var url = Endpoint;
            if (!string.IsNullOrEmpty(currentVersion))
            {
                url += $"?currentVersion={UnityWebRequest.EscapeURL(currentVersion)}";
            }

            using var request = UnityWebRequest.Get(url);
            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError($"[{GetType().Name}] Request failed: {request.error}");
                onSuccess?.Invoke(default);
                yield break;
            }

            var rawJson = request.downloadHandler.text;
            OnSuccessRawJson(rawJson);

            // A parse failure must still reach the callback: callers block on it, so throwing here would
            // kill this coroutine and leave them waiting forever.
            if (!JsonCodec.TryDeserialize(rawJson, out TResponse response, out string error))
            {
                WDebug.LogError($"[{GetType().Name}] Parse failed: {error} / {JsonCodec.Excerpt(rawJson)}");
                onSuccess?.Invoke(default);
                yield break;
            }

            onSuccess?.Invoke(response);
        }
    }
}

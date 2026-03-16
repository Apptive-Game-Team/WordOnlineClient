using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Data.Versioning
{
    public abstract class VersionedApiClient<TResponse>
    {
        protected abstract string Endpoint { get; }

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

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                WDebug.LogError($"[{GetType().Name}] Request failed: {request.error}");
                onSuccess?.Invoke(default);
            }
        }
    }
}

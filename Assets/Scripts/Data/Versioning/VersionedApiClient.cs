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
        private const int DefaultTimeoutSeconds = 10;

        protected abstract string Endpoint { get; }
        public string SourceUrl => Endpoint;

        /// <summary>
        /// Seconds to wait before the request is abandoned. Without one a stalled connection never
        /// completes, and GameDataRefresh waits on this coroutine for the rest of the session.
        /// </summary>
        protected virtual int TimeoutSeconds => DefaultTimeoutSeconds;

        protected virtual void OnSuccessRawJson(string json) { }

        public IEnumerator Get(Action<VersionedFetchResult<TResponse>> onCompleted, string currentVersion = null)
        {
            var url = Endpoint;
            if (!string.IsNullOrEmpty(currentVersion))
            {
                url += $"?currentVersion={UnityWebRequest.EscapeURL(currentVersion)}";
            }

            using var request = UnityWebRequest.Get(url);
            request.timeout = TimeoutSeconds;
            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onCompleted?.Invoke(ClassifyFailure(request));
                yield break;
            }

            var rawJson = request.downloadHandler.text;
            OnSuccessRawJson(rawJson);

            // A parse failure must still reach the callback: callers block on it, so throwing here would
            // kill this coroutine and leave them waiting forever.
            if (!JsonCodec.TryDeserialize(rawJson, out TResponse response, out string error))
            {
                WDebug.LogError($"[{GetType().Name}] Parse failed: {error} / {JsonCodec.Excerpt(rawJson)}");
                onCompleted?.Invoke(VersionedFetchResult<TResponse>.ParseFailed(request.responseCode, error));
                yield break;
            }

            onCompleted?.Invoke(VersionedFetchResult<TResponse>.Succeeded(response, request.responseCode));
        }

        private VersionedFetchResult<TResponse> ClassifyFailure(UnityWebRequest request)
        {
            string error = request.error ?? string.Empty;

            switch (request.result)
            {
                case UnityWebRequest.Result.ProtocolError:
                    WDebug.LogError($"[{GetType().Name}] Server answered {request.responseCode}: {error}");
                    return VersionedFetchResult<TResponse>.HttpError(request.responseCode, error);

                case UnityWebRequest.Result.DataProcessingError:
                    WDebug.LogError($"[{GetType().Name}] Response body unusable: {error}");
                    return VersionedFetchResult<TResponse>.ParseFailed(request.responseCode, error);

                default:
                    // Unity has no Result value of its own for a timeout: it reports one as a
                    // ConnectionError whose message is "Request timeout".
                    if (error.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        WDebug.LogWarning($"[{GetType().Name}] No answer within {TimeoutSeconds}s from {Endpoint}");
                        return VersionedFetchResult<TResponse>.TimedOut(error);
                    }

                    WDebug.LogWarning($"[{GetType().Name}] Could not reach {Endpoint}: {error}");
                    return VersionedFetchResult<TResponse>.ConnectionFailed(error);
            }
        }
    }
}

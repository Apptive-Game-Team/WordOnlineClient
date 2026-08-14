using System;
using System.Collections;
using Global;
using Global.Serialization;
using UnityEngine.Networking;

namespace Data
{
    /// <summary>
    /// Checks whether a matching server can serve traffic right now.
    ///
    /// This is deliberately separate from <c>LoginScene.DeployStatusChecker</c>: that one pops a
    /// localized system message for every non-healthy state, which is wrong when probing several
    /// candidate servers at once just to build a dropdown.
    /// </summary>
    public static class MatchingServerHealthProbe
    {
        private const int TimeoutSeconds = 3;
        private const string HealthyStatus = "HEALTHY";

        [Serializable]
        private class DeployStatusDto
        {
            public string status;
        }

        public static IEnumerator Probe(Server server, Action<bool> onResult)
        {
            using UnityWebRequest request = UnityWebRequest.Get(server.Api.Path("api", "deploy", "status"));
            request.timeout = TimeoutSeconds;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WDebug.Log($"[MatchingServerHealthProbe] {server.host} unreachable: {request.error}");
                onResult?.Invoke(false);
                yield break;
            }

            onResult?.Invoke(IsHealthy(request.downloadHandler.text, server));
        }

        private static bool IsHealthy(string responseBody, Server server)
        {
            if (!JsonCodec.TryDeserialize(responseBody, out DeployStatusDto deployStatus, out string parseError))
            {
                WDebug.LogWarning($"[MatchingServerHealthProbe] {server.host} returned unparsable status: {parseError}");
                return false;
            }

            if (deployStatus == null || string.IsNullOrEmpty(deployStatus.status))
                return false;

            if (deployStatus.status.ToUpperInvariant() == HealthyStatus)
                return true;

            WDebug.Log($"[MatchingServerHealthProbe] {server.host} status is {deployStatus.status}");
            return false;
        }
    }
}

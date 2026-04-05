using System;
using System.Collections;
using Data;
using Global;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;

namespace LobbyScene
{
    public static class DeployStatusChecker
    {
        [Serializable]
        private class DeployStatusDto
        {
            public string status;
        }

        private static readonly LocalizedString serverMaintenance = new LocalizedString { TableReference = "SystemMessageUI", TableEntryReference = "serverMaintenance" };
        private static readonly LocalizedString serverDown = new LocalizedString { TableReference = "SystemMessageUI", TableEntryReference = "serverDown" };
        private static readonly LocalizedString serverDeploying = new LocalizedString { TableReference = "SystemMessageUI", TableEntryReference = "serverDeploying" };

        /// <summary>
        /// Checks the deploy status of the lobby server.
        /// Invokes onResult with true if the server is Healthy, false otherwise.
        /// Displays an appropriate localized message for non-Healthy states.
        /// </summary>
        public static IEnumerator CheckDeployStatus(Action<bool> onResult)
        {
            var url = ServerList.MatchingServer.url + "/api/deploy/status";

            using var www = UnityWebRequest.Get(url);
            Server.SetAcceptLanguage(www);
            www.timeout = 10;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError($"[CheckDeployStatus] fail: {www.responseCode} / {www.error}");
                SystemMessageUI.Instance.ShowMessage(serverDown);
                onResult?.Invoke(false);
                yield break;
            }

            DeployStatusDto dto = null;
            try
            {
                dto = JsonUtility.FromJson<DeployStatusDto>(www.downloadHandler.text);
            }
            catch (Exception e)
            {
                WDebug.LogError($"[CheckDeployStatus] JSON parse error: {e}\n{www.downloadHandler.text}");
                SystemMessageUI.Instance.ShowMessage(serverDown);
                onResult?.Invoke(false);
                yield break;
            }

            if (dto == null || string.IsNullOrEmpty(dto.status))
            {
                WDebug.LogWarning("[CheckDeployStatus] empty or null status");
                SystemMessageUI.Instance.ShowMessage(serverDown);
                onResult?.Invoke(false);
                yield break;
            }

            WDebug.Log("[CheckDeployStatus] server status: " + dto.status);

            switch (dto.status.ToUpperInvariant())
            {
                case "HEALTHY":
                    onResult?.Invoke(true);
                    break;
                case "MAINTENANCE":
                    SystemMessageUI.Instance.ShowMessage(serverMaintenance);
                    onResult?.Invoke(false);
                    break;
                case "DEPLOYING":
                    SystemMessageUI.Instance.ShowMessage(serverDeploying);
                    onResult?.Invoke(false);
                    break;
                case "DOWN":
                default:
                    SystemMessageUI.Instance.ShowMessage(serverDown);
                    onResult?.Invoke(false);
                    break;
            }
        }
    }
}

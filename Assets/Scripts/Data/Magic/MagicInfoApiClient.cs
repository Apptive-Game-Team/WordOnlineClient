using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Data.Magic
{
    public class MagicInfoApiClient
    {
        public IEnumerator GetMagicInfo(Action<MagicInfoResponse> onSuccess, string currentVersion = null)
        {
            var url = $"{ServerList.MatchingServer.url}/api/data/magics";
            if (!string.IsNullOrEmpty(currentVersion))
            {
                url += $"?currentVersion={UnityWebRequest.EscapeURL(currentVersion)}";
            }

            using var webRequest = UnityWebRequest.Get(url);
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<MagicInfoResponse>(webRequest.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                WDebug.LogError($"Failed to get magic info: {webRequest.error}");
                onSuccess?.Invoke(null);
            }
        }
    }
}

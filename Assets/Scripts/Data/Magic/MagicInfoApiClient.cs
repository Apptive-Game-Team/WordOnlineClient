using System;
using System.Collections;
using Data.Util;
using Global;
using UnityEngine;
using UnityEngine.Networking;

namespace Data.Magic
{
    public class MagicInfoApiClient : MonoBehaviour
    {
        public void GetMagicInfo(Action<MagicInfoResponse> callback, long? currentVersion = null)
        {
            StartCoroutine(_GetMagicInfo(callback, currentVersion));
        }

        private static IEnumerator _GetMagicInfo(Action<MagicInfoResponse> callback, long? currentVersion)
        {
            var versionParameter = new VersionParameter(currentVersion);
            string url = versionParameter.AppendToUrl($"{ServerList.MatchingServer.url}/api/data/magics");

            using UnityWebRequest webRequest = UnityWebRequest.Get(url);

            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);

            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                MagicInfoResponse response = JsonUtility.FromJson<MagicInfoResponse>(webRequest.downloadHandler.text);
                callback.Invoke(response);
            }
            else
            {
                WDebug.LogError($"Failed to get magic info: {webRequest.error}");
                callback.Invoke(null);
            }
        }
    }
}

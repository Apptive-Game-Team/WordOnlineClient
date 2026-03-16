using System;
using System.Collections;
using Data.Versioning;
using Global;
using UnityEngine;
using UnityEngine.Networking;

namespace Data.Magic
{
    public class MagicInfoApiClient : MonoBehaviour
    {
        private const string MagicsVersionKey = "magics_version";
        private static readonly VersionParameter _versionParameter = new VersionParameter(MagicsVersionKey);

        public void GetMagicInfo(Action<MagicInfoResponse> callback)
        {
            StartCoroutine(_GetMagicInfo(callback));
        }

        private static IEnumerator _GetMagicInfo(Action<MagicInfoResponse> callback)
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get(
                $"{ServerList.MatchingServer.url}/api/data/magics{_versionParameter.ToQueryString()}"
            );

            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);

            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                MagicInfoResponse response = JsonUtility.FromJson<MagicInfoResponse>(
                    webRequest.downloadHandler.text
                );

                if (response != null && !string.IsNullOrEmpty(response.version))
                {
                    _versionParameter.Save(response.version);
                }

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

using System;
using System.Collections;
using Global;
using UnityEngine;
using UnityEngine.Networking;

namespace Data.Param
{
    public class ParamApiClient
    {
        public IEnumerator GetParameters(Action<ParametersResponse> onSuccess, string currentVersion = null)
        {
            var url = $"{ServerList.MatchingServer.url}/api/data/parameters";
            if (!string.IsNullOrEmpty(currentVersion))
            {
                url += $"?currentVersion={currentVersion}";
            }

            using var www = UnityWebRequest.Get(url);
            Server.SetAcceptLanguage(www);
            Server.SetAuthorization(www);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var json = www.downloadHandler.text;
                var response = JsonUtility.FromJson<ParametersResponse>(json);
                onSuccess?.Invoke(response);
            }
            else
            {
                WDebug.LogError($"Failed to get parameters: {www.error}");
                onSuccess?.Invoke(null);
            }
        }
    }
}

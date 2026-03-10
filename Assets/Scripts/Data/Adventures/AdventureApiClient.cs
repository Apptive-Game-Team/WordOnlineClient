using System;
using System.Collections;
using Global;
using UnityEngine;
using UnityEngine.Networking;

namespace Data.Adventures
{
    public class AdventureApiClient : AdventureClient
    {

        public override IEnumerator GetAdventure(Action<AdventuresResponse> callback)
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get($"{ServerList.MatchingServer.url}/api/adventures");
            
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                AdventuresResponse adventuresResponse = JsonUtility.FromJson<AdventuresResponse>(jsonResponse);
                callback.Invoke(adventuresResponse);
            }
            else
            {
                WDebug.LogError($"Error fetching adventures: {webRequest.error}");
                callback.Invoke(null);
            }
        }
    }
}
using System;
using System.Collections;
using Global;
using UnityEngine;
using UnityEngine.Networking;
using Data.Adventures.Dto;
using Global.Serialization;

namespace Data.Adventures
{
    public class AdventureApiClient : AdventureClient
    {
        public override IEnumerator GetAdventure(Action<AdventuresResponseDto> callback)
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get($"{ServerList.MatchingServer.url}/api/adventures");
            
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            
            yield return webRequest.SendWebRequest();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                if (!JsonCodec.TryDeserialize(jsonResponse, out AdventuresResponseDto adventuresResponseDto, out string error))
                {
                    WDebug.LogError($"Error parsing adventures: {error} / {JsonCodec.Excerpt(jsonResponse)}");
                }

                callback.Invoke(adventuresResponseDto);
            }
            else
            {
                WDebug.LogError($"Error fetching adventures: {webRequest.error}");
                callback.Invoke(null);
            }
        }
    }
}
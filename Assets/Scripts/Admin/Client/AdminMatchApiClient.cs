using System;
using System.Collections;
using System.Text;
using Admin.Dto;
using Data;
using Global;
using UnityEngine;
using UnityEngine.Networking;
using Global.Serialization;

namespace Admin.Client
{
    public class AdminMatchApiClient
    {
        public IEnumerator MatchBots(BotMatchRequestDto dto, Action<MatchedInfoDto> callback)
        {
            string url = $"{ServerList.MatchingServer.url}/api/admin/matches/bots";
            string json = JsonCodec.Serialize(dto);

            using UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError($"MatchBots error: {request.error}\nURL: {url}\nResponse: {request.downloadHandler.text}");
                callback?.Invoke(null);
                yield break;
            }

            MatchedInfoDto matchedInfoDto = JsonCodec.Deserialize<MatchedInfoDto>(request.downloadHandler.text);
            callback?.Invoke(matchedInfoDto);
        }
    }
}

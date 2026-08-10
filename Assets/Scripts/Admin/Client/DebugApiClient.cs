using System;
using System.Collections;
using System.Text;
using Admin.Dto;
using Data;
using Global;
using Global.Util;
using UnityEngine;
using UnityEngine.Networking;
using Global.Serialization;

namespace Admin.Client
{
    public class DebugApiClient
    {
        private string BaseUrl => SceneContext.MatchInfo.server + "/api/debug";

        public IEnumerator SummonMagic(DebugSummonMagicRequestDto dto, Action<DebugActionResponseDto> callback)
        {
            string url = $"{BaseUrl}/magic";
            string json = JsonCodec.Serialize(dto);
            
            yield return PostJson(url, json, (responseJson) =>
            {
                if (responseJson == null)
                {
                    callback?.Invoke(new DebugActionResponseDto { success = false, message = "Network error" });
                }
                else if (!JsonCodec.TryDeserialize(responseJson, out DebugActionResponseDto response, out string error))
                {
                    WDebug.LogError($"Debug action parse error: {error} / {JsonCodec.Excerpt(responseJson)}");
                    callback?.Invoke(new DebugActionResponseDto { success = false, message = "Invalid response" });
                }
                else
                {
                    callback?.Invoke(response);
                }
            });
        }

        public IEnumerator SpawnPrefab(DebugSpawnPrefabRequestDto dto, Action<DebugActionResponseDto> callback)
        {
            string url = $"{BaseUrl}/prefab";
            string json = JsonCodec.Serialize(dto);
            
            yield return PostJson(url, json, (responseJson) =>
            {
                if (responseJson == null)
                {
                    callback?.Invoke(new DebugActionResponseDto { success = false, message = "Network error" });
                }
                else if (!JsonCodec.TryDeserialize(responseJson, out DebugActionResponseDto response, out string error))
                {
                    WDebug.LogError($"Debug action parse error: {error} / {JsonCodec.Excerpt(responseJson)}");
                    callback?.Invoke(new DebugActionResponseDto { success = false, message = "Invalid response" });
                }
                else
                {
                    callback?.Invoke(response);
                }
            });
        }

        public IEnumerator GetMagics(Action<DebugMagicDto[]> callback)
        {
            string url = $"{BaseUrl}/magics";
            using UnityWebRequest request = UnityWebRequest.Get(url);
            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching magics: {request.error}");
                callback?.Invoke(null);
            }
            else
            {
                string body = request.downloadHandler.text;
                if (!JsonCodec.TryDeserialize(body, out DebugMagicDto[] magics, out string error))
                {
                    WDebug.LogError($"Error parsing magics: {error} / {JsonCodec.Excerpt(body)}");
                }

                callback?.Invoke(magics);
            }
        }

        public IEnumerator GetPrefabs(Action<DebugPrefabDto[]> callback)
        {
            string url = $"{BaseUrl}/prefabs";
            using UnityWebRequest request = UnityWebRequest.Get(url);
            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError($"Error fetching prefabs: {request.error} {request.downloadHandler.text}");
                callback?.Invoke(null);
            }
            else
            {
                string body = request.downloadHandler.text;
                if (!JsonCodec.TryDeserialize(body, out DebugPrefabDto[] prefabs, out string error))
                {
                    WDebug.LogError($"Error parsing prefabs: {error} / {JsonCodec.Excerpt(body)}");
                }

                callback?.Invoke(prefabs);
            }
        }

        private IEnumerator PostJson(string url, string json, Action<string> callback)
        {
            using UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            Server.SetAuthorization(request);
            Server.SetAcceptLanguage(request);
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}\nURL: {url}\nResponse: {request.downloadHandler.text}");
                callback?.Invoke(null);
            }
            else
            {
                callback?.Invoke(request.downloadHandler.text);
            }
        }
    }
}

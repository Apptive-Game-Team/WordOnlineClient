using System;
using System.Collections;
using Scripts.Admin.Dto;
using Scripts.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace Scripts.Admin.Client
{
    public class RoomApiClient
    {
        public IEnumerator GetRoomList(Action<RoomList> callback)
        {
            string url = ServerList.MatchingServer.url + "/api/game-sessions";
            
            using UnityWebRequest request = UnityWebRequest.Get(url);
            
            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching room list: {request.error}");
                callback(null);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                RoomList roomList = JsonUtility.FromJson<RoomList>(jsonResponse);
                callback(roomList);
            }
        }
    }
}
using System;
using System.Collections;
using Data;
using Data.Net;
using Global;
using UnityEngine;
using UnityEngine.Networking;
using Global.Serialization;

namespace LobbyScene
{
    public class MatchQueueApiService : MonoBehaviour
    {
        private static ServerEndpoint MatchTickets =>
            ServerList.MatchingServer.Api.Path("api", "match", "tickets");

        public IEnumerator CreateTicket(Action<MatchTicket> callback)
        {
            using var webRequest = new UnityWebRequest(MatchTickets, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return SendTicketRequest(webRequest, callback, "CreateTicket");
        }

        public IEnumerator GetActiveTicket(Action<bool, MatchTicket> callback)
        {
            using var webRequest = UnityWebRequest.Get(MatchTickets.Path("active"));
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            yield return webRequest.SendWebRequest();

            if (webRequest.responseCode == 404)
            {
                callback(true, null);
                yield break;
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError($"GetActiveTicket error: {webRequest.responseCode} / {webRequest.error}");
                callback(false, null);
                yield break;
            }

            callback(true, JsonCodec.Deserialize<MatchTicket>(webRequest.downloadHandler.text));
        }

        public IEnumerator CancelTicket(string ticketId, Action<MatchCancelResult> callback)
        {
            using var webRequest = UnityWebRequest.Delete(MatchTickets.Path(ticketId));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError($"CancelTicket error: {webRequest.responseCode} / {webRequest.error}");
                callback(null);
                yield break;
            }

            callback(JsonCodec.Deserialize<MatchCancelResult>(webRequest.downloadHandler.text));
        }

        private static IEnumerator SendTicketRequest(
            UnityWebRequest webRequest,
            Action<MatchTicket> callback,
            string operation)
        {
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError($"{operation} error: {webRequest.responseCode} / {webRequest.error}");
                callback(null);
                yield break;
            }

            callback(JsonCodec.Deserialize<MatchTicket>(webRequest.downloadHandler.text));
        }

        public IEnumerator MatchPractice(Action<MatchedInfoDto> callback)
        {
            using var webRequest = UnityWebRequest.Get(ServerList.MatchingServer.Api.Path("api", "match", "practice", "me"));
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                MatchedInfoDto matchedInfoDto = JsonCodec.Deserialize<MatchedInfoDto>(webRequest.downloadHandler.text);
                callback(matchedInfoDto);
            }
            else
            {
                WDebug.LogError($"MatchPractice error: {webRequest.error}");
                callback(null);
            }
        }

    }
}

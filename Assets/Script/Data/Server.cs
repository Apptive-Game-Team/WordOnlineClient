using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Script.Global;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;

namespace Script.Data
{
    public class Server
    {
        public Server(string name, string host, int port, bool isSecure = false)
        {
            this.name = name;
            this.host = host;
            this.port = port;
            this.isSecure = isSecure;
        }

        public string name;
        public string host;
        public int port;
        public bool isSecure;
        public string url => $"{(isSecure ? "https" : "http")}://{host}:{port}";
        public string webSocketUrl => $"{(isSecure ? "wss" : "ws")}://{host}:{port}/ws?token=";
        
        public static void SetAcceptLanguage(UnityWebRequest req)
        {
            var locale = LocalizationSettings.SelectedLocale;
            
            var lang = locale?.Identifier.Code;

            if (!string.IsNullOrWhiteSpace(lang))
            {
                req.SetRequestHeader("Accept-Language", lang);
            }
            else
            {
                WDebug.LogWarning("Accept-Language header skipped (null or empty).");
            }
        }
        
        public static void SetAuthorization(UnityWebRequest webRequest)
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
        }
        
        public IEnumerator GetPing(Action<int> callback)
        {
            Stopwatch stopwatch = new Stopwatch();
            using var www = new UnityWebRequest($"{url}/healthcheck", "GET");
            
            stopwatch.Start();
            yield return www.SendWebRequest();
            stopwatch.Stop();

            if (www.result == UnityWebRequest.Result.Success)
            {
                int ping = (int)stopwatch.ElapsedMilliseconds;
                callback?.Invoke(ping);
            }
            else
            {
                callback?.Invoke(-1);
            }
        }
    }
    
    public static class ServerList
    {
        public static readonly List<Server> Servers = new()
        {
            new Server("춘천", "www.monolong.shop", 7777, true),
            new Server("춘천2", "www.monolong.shop", 6210, true),
            new Server("로컬", "localhost", 7777)
        };

        public static readonly Server MatchingServer = new Server("춘천", "www.monolong.shop", 6210, true);
        // public static readonly Server MatchingServer = new Server("춘천", "localhost", 6209, false);
        
        public static readonly Server AccountServer = new Server("춘천", "www.monolong.shop", 6203, true);
    }
}
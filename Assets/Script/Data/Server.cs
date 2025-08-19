using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static List<Server> servers = new()
        {
            new Server("시드니", "www.monolong.shop", 7777, true),
            new Server("미국-서부-mn", "game.monolong.shop", 7777, true),
            new Server("미국-서부-ys", "google.yunseong.shop", 7777, true),
            new Server("로컬", "localhost", 7777)
        };
    }
}
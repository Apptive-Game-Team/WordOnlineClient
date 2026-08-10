using System;
using System.Collections;
using System.Diagnostics;
using Data.Net;
using Global;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;

namespace Data
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

        /// <summary>
        /// REST 요청용 엔드포인트. 경로와 쿼리는 <see cref="ServerEndpoint"/>로 이어 붙인다.
        /// </summary>
        public ServerEndpoint Api => ServerEndpoint.Of(url);

        /// <summary>STOMP 등 웹소켓 접속용 엔드포인트.</summary>
        public ServerEndpoint WebSocket => Api.AsWebSocket();

        /// <summary>
        /// 문자열 보간으로 URL을 조립하는 기존 호출부용 호환 프로퍼티.
        /// 새 코드는 <see cref="Api"/>를 쓴다.
        /// </summary>
        public string url => $"{(isSecure ? "https" : "http")}://{host}:{port}";

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
            using var www = new UnityWebRequest(Api.Path("healthcheck"), "GET");
            
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
}
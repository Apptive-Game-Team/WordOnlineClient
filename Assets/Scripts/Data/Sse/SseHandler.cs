using System;
using Global;
using UnityEngine;
using UnityEngine.Networking;

namespace Data.Sse
{

    public class SseHandler : MonoBehaviour
    {
        private UnityWebRequest request;
        private Action<string> onMessage;

        public void StartSse(string url, Action<string> callback)
        {
            onMessage = callback;
            request = new UnityWebRequest(url, "GET");

            request.downloadHandler = new SseDownloadHandler(OnReceive);
            Server.SetAuthorization(request);
            Server.SetAcceptLanguage(request);
            request.SetRequestHeader("Cache-Control", "no-cache");
            request.timeout = 0;
            
            request.SendWebRequest();
        }

        private void OnReceive(string line)
        {
            if (line.Contains("heartbeat"))
            {
                WDebug.Log("SSE Heartbeat received.");
                return;
            }
            // Unity 메인 스레드에서 실행
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                onMessage?.Invoke(line);
            });
        }

        private void OnDestroy()
        {
            if (request != null)
            {
                request.Abort();
                request.Dispose();
            }
        }
    }

}
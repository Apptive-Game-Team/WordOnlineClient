#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using System;
using UnityEngine;
using Global.Serialization;

namespace Global.Stomp
{
    /// <summary>
    /// WebGL 빌드 전용 STOMP 전송 구현.
    /// JavaScript(StompPlugin.jslib)와 Unity SendMessage로 통신.
    ///
    /// JS → C# 콜백 메서드: OnConnected, OnError, OnDisconnected, OnSubscriptionMessage
    /// (StompConnector GameObject에 SendMessage로 전달됨)
    /// </summary>
    public class WebGLStompTransport : MonoBehaviour, IStompTransport
    {
        public bool IsConnected { get; private set; }

        public event Action Connected;
        public event Action<string> Disconnected;
        public event Action<string> Errored;
        public event Action<string, string> MessageReceived;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ConnectStompSocket(string url, string token);

        [DllImport("__Internal")]
        private static extern void SubscribeStomp(string topic, string subscriptionId);

        [DllImport("__Internal")]
        private static extern void SendStomp(string topic, string message);

        [DllImport("__Internal")]
        private static extern void UnsubscribeStomp(string subscriptionId);

        [DllImport("__Internal")]
        private static extern void DisconnectStomp();
#endif

        public void Connect(string url, string token)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ConnectStompSocket(url, token);
#endif
        }

        public void Subscribe(string topic, string subscriptionId)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SubscribeStomp(topic, subscriptionId);
#endif
        }

        public void Unsubscribe(string subscriptionId)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            UnsubscribeStomp(subscriptionId);
#endif
        }

        public void Send(string topic, string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SendStomp(topic, message);
#endif
        }

        public void Disconnect()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            DisconnectStomp();
#endif
        }

        // ─── JS → C# 콜백 (Unity SendMessage로 호출됨) ───────────────────────

        /// <summary>JS: SendMessage("StompConnector", "OnConnected", frame)</summary>
        private void OnConnected(string frame)
        {
            WDebug.Log("[STOMP:WebGL] 연결됨");
            IsConnected = true;
            Connected?.Invoke();
        }

        /// <summary>JS: SendMessage("StompConnector", "OnError", error)</summary>
        private void OnError(string error)
        {
            WDebug.LogError("[STOMP:WebGL] 에러: " + error);
            IsConnected = false;
            Errored?.Invoke(error);
        }

        /// <summary>JS: SendMessage("StompConnector", "OnDisconnected", message)</summary>
        private void OnDisconnected(string message)
        {
            WDebug.Log("[STOMP:WebGL] 연결 종료: " + message);
            IsConnected = false;
            Disconnected?.Invoke(message);
        }

        /// <summary>
        /// JS: SendMessage("StompConnector", "OnSubscriptionMessage", json)
        /// json 형식: {"id":"frame-sub","body":"..."}
        /// </summary>
        private void OnSubscriptionMessage(string json)
        {
            if (!JsonCodec.TryDeserialize(json, out SubscriptionMessageDto msg))
            {
                return;
            }

            MessageReceived?.Invoke(msg.id, msg.body);
        }

        [Serializable]
        private class SubscriptionMessageDto
        {
            public string id;
            public string body;
        }
    }
}

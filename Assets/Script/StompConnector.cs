using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Script.Data.Util;
using UnityEngine;
using Script.GameScene.Handler;
using Script.Global;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class StompConnector : LocalSingletonObject<StompConnector>
{
    private static bool isConnected = false;
    
    private class ConnectionInfo
    {
        internal string topic;
        internal string callback;
        internal string subscriptionId;
        
        public ConnectionInfo(string topic, string callback, string subscriptionId)
        {
            this.topic = topic;
            this.callback = callback;
            this.subscriptionId = subscriptionId;
        }
    }
    
    private readonly List<ConnectionInfo> _connections = new List<ConnectionInfo>();
    
    public LocalizedString notConnectedToServer;
    public LocalizedString matchingInProgress;
    public LocalizedString matchingFailed;
    public LocalizedString connectionClosed;
    public LocalizedString connectionDelayed;

    protected override void Awake()
    {
        gameObject.name = "StompConnector";
        base.Awake();
    }

    private void Start()
    {
        ConnectToServer();
        StartInGameFlow(SceneContext.MatchInfo.sessionId);
    }
    
    private void OnDestroy()
    {
        DisconnectFromServer();
    }

    // WebGL에서 JavaScript 함수 호출
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void ConnectStompSocket(string url, string jwtToken);

    [DllImport("__Internal")]
    private static extern void SubscribeStomp(string topic, string callback, string subscriptionId);

    [DllImport("__Internal")]
    private static extern void SendStomp(string topic, string message);

    [DllImport("__Internal")]
    private static extern void UnsubscribeStomp(string subscriptionId);

    [DllImport("__Internal")]
    private static extern void DisconnectStomp();
    #endif
    
    public void StartInGameFlow(string sessionId)
    {
        CheckConnection();

        UnsubscribeFromTopic("match-sub");
        SubscribeToTopic($"/game/{sessionId}/frameInfos/{SceneContext.UserID}", "OnFrameInfoReceived", "frame-sub");
    }

    private void CheckConnection()
    {
        if (!isConnected)
        {
            SystemMessageUI.Instance.ShowMessage(notConnectedToServer);
            WDebug.LogError("STOMP 서버에 연결되지 않았습니다.");
            OnError("Not connected");
            throw new Exception("Not connected");
        }
        else
        {
            
        }
    }
    
    // 연결 상태 처리
    public void OnConnected(string frame)
    {
        WDebug.Log("STOMP 연결됨: " + frame);
        isConnected = true;
    }
    
    // 메시지 수신 처리
    public void OnMessageReceived(string message)
    {
        WDebug.Log("매칭 메시지 수신: " + message);

        var matchInfo = JsonUtility.FromJson<MatchedInfoDto>(message);
        SceneContext.MatchInfo = matchInfo;

        if (!string.IsNullOrEmpty(matchInfo.sessionId))
        {
            WDebug.Log("매칭 완료! 세션 ID: " + matchInfo.sessionId);
            SceneManager.LoadScene("GameScene");
            StartInGameFlow(matchInfo.sessionId);
        }
        else
        {
            if (message.Contains("Successfully"))
            {
                MatchingStatusTextUI.SetMatchingStatusText(matchingInProgress);
            } else if (message.Contains("Failed"))
            {
                MatchingStatusTextUI.SetMatchingStatusText(matchingFailed);
                UnsubscribeFromTopic("match-sub");
            }
        }
    }
    
    // 연결 종료 처리
    public void OnDisconnected(string message)
    {
        SystemMessageUI.Instance.ShowMessage(connectionClosed);
        WDebug.Log("STOMP 연결 종료: " + message);
        isConnected = false;
    }

    // 연결 실패 처리
    public void OnError(string error)
    {
        SystemMessageUI.Instance.ShowMessage(connectionDelayed);
        WDebug.LogError("STOMP 에러: " + error);
        isConnected = false;
        
        Invoke(nameof(ConnectToServer), 1);
    }

    // WebSocket 서버에 연결
    public void ConnectToServer()
    {
        string url = UrlUtil.httpToWebSocket(SceneContext.MatchInfo.server, SceneContext.JwtToken);
        
        if (!isConnected)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            ConnectStompSocket(url, SceneContext.JwtToken);
            #endif
            Reconnect();
        }
        else
        {
            WDebug.LogWarning("이미 연결되어 있습니다.");
        }
    }

    private void Reconnect()
    {
        foreach (var connection in _connections)
        {
            SubscribeToTopic(connection.topic, connection.callback, connection.subscriptionId);
        }
    }

    // 메시지 전송
    public void SendMessageToServer(string topic, string message)
    {
        if (isConnected)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            SendStomp(topic, message);
            #endif
        }
        else
        {
            SystemMessageUI.Instance.ShowMessage(notConnectedToServer);
            WDebug.LogError("STOMP 서버에 연결되지 않았습니다.");
        }
    }

    // 구독 처리
    public void SubscribeToTopic(string topic, string callback, string subscriptionId)
    {
        if (isConnected)
        {
            WDebug.Log("Subscribe to topic: " + topic + " with subscriptionId: " + subscriptionId);
            #if UNITY_WEBGL && !UNITY_EDITOR
            SubscribeStomp(topic, callback, subscriptionId);
            #endif
            _connections.Add(new ConnectionInfo(topic, callback, subscriptionId));
        }
        else
        {
            SystemMessageUI.Instance.ShowMessage(notConnectedToServer);
            WDebug.LogError("STOMP 서버에 연결되지 않았습니다.");
        }
    }

    // 구독 해제 처리
    public void UnsubscribeFromTopic(string subscriptionId)
    {
        if (isConnected)
        {
            WDebug.Log("Unsubscribe from topic: " + subscriptionId);
            #if UNITY_WEBGL && !UNITY_EDITOR
            UnsubscribeStomp(subscriptionId);
            #endif
            _connections.Remove(_connections.Find(c => c.subscriptionId == subscriptionId));
        }
        else
        {
            SystemMessageUI.Instance.ShowMessage(notConnectedToServer);
            WDebug.LogError("STOMP 서버에 연결되지 않았습니다.");
        }
    }

    // STOMP 서버와 연결 종료
    public void DisconnectFromServer()
    {
        if (isConnected)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            DisconnectStomp();
            #endif
            _connections.Clear();
        }
        else
        {
            SystemMessageUI.Instance.ShowMessage(notConnectedToServer);
            WDebug.LogError("STOMP 서버에 연결되지 않았습니다.");
        }
    }

    private IFrameInfoHandler<string> frameInfoHandler = new GeneralHandler();
    
    public void OnFrameInfoReceived(string json)
    {
        // WDebug.Log("FrameInfo 수신: " + json);
        
        frameInfoHandler.Handler(json);
    }
}

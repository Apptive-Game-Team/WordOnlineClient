using System;
using UnityEngine;
using System.Runtime.InteropServices;
using Script.GameScene;
using UnityEngine.SceneManagement;

public class StompConnector : MonoBehaviour
{
    private static bool isConnected = false;
    public static StompConnector Instance;
    private void Awake()
    {
        gameObject.name = "StompConnector";

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    // WebGL에서 JavaScript 함수 호출
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void ConnectStompSocket(string url);

    [DllImport("__Internal")]
    private static extern void SubscribeStomp(string topic, string callback, string subscriptionId);

    [DllImport("__Internal")]
    private static extern void SendStomp(string topic, string message);

    [DllImport("__Internal")]
    private static extern void UnsubscribeStomp(string subscriptionId);

    [DllImport("__Internal")]
    private static extern void DisconnectStomp();
    #endif

    // 연결 상태 처리
    public void OnConnected(string frame)
    {
        Debug.Log("STOMP 연결됨: " + frame);
        isConnected = true;
        
        // 예: 메시지 오면 OnMessageReceived 호출됨
        SubscribeToTopic(
            $"/queue/match-status/{SceneContext.UserID}",
            "OnMessageReceived",
            "match-sub"
        );
        // 예: 서버로 JSON 메시지 전송
        SendMessageToServer(
            "/app/game/match/queue",
            SceneContext.UserID
        );
    }


    
    // 메시지 수신 처리
    public void OnMessageReceived(string message)
    {
        Debug.Log("매칭 메시지 수신: " + message);

        var matchInfo = JsonUtility.FromJson<MatchedInfoDto>(message);
        SceneContext.MatchInfo = matchInfo;

        if (!string.IsNullOrEmpty(matchInfo.sessionId))
        {
            Debug.Log("매칭 완료! 세션 ID: " + matchInfo.sessionId);

            // 씬 전환 (예: GameScene)
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            SubscribeToTopic($"/game/{SceneContext.MatchInfo.sessionId}/frameInfos/{SceneContext.UserID}",
                "OnFrameInfoReceived",
                "frame-sub");
        }
    }

    // 연결 종료 처리
    public void OnDisconnected(string message)
    {
        Debug.Log("STOMP 연결 종료: " + message);
        isConnected = false;
    }

    // 연결 실패 처리
    public void OnError(string error)
    {
        Debug.LogError("STOMP 에러: " + error);
    }

    // WebSocket 서버에 연결
    public void ConnectToServer(string url)
    {
        if (!isConnected)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            ConnectStompSocket(url);
            #endif
        }
        else
        {
            Debug.LogWarning("이미 연결되어 있습니다.");
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
            Debug.LogError("STOMP 서버에 연결되지 않았습니다.");
        }
    }

    // 구독 처리
    public void SubscribeToTopic(string topic, string callback, string subscriptionId)
    {
        if (isConnected)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            SubscribeStomp(topic, callback, subscriptionId);
            #endif
        }
        else
        {
            Debug.LogError("STOMP 서버에 연결되지 않았습니다.");
        }
    }

    // 구독 해제 처리
    public void UnsubscribeFromTopic(string subscriptionId)
    {
        if (isConnected)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            UnsubscribeStomp(subscriptionId);
            #endif
        }
        else
        {
            Debug.LogError("STOMP 서버에 연결되지 않았습니다.");
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
        }
        else
        {
            Debug.LogError("STOMP 서버에 연결되지 않았습니다.");
        }
    }

    [System.Serializable]
    public class TypeChecker
    {
        public string type;
    }

    [System.Serializable]
    public class MagicValidInfo
    {
        public string type;
        public bool valid;
        public int updateMana;
        public int id;
    }
    
    public void OnFrameInfoReceived(string json)
    {
        Debug.Log("FrameInfo 수신: " + json);

        TypeChecker infotype = JsonUtility.FromJson<TypeChecker>(json);

        
        switch (infotype.type)
        {
            case "frame":
                FrameInfo info = JsonUtility.FromJson<FrameInfo>(json);
                
                // 마나 UI 업데이트
                GameSceneUIController.Instance.UpdateMana(info.updatedMana);
                //
                // // 카드 추가
                foreach (string cardName in info.cards.added)
                    GameSceneUIController.Instance.AddCard(cardName);
                //
                // // 생성된 오브젝트 배치
                foreach (var created in info.objects.create)
                    ObjectSpawner.Instance.SpawnObject(created);
        
                // // 기존 오브젝트 업데이트
                foreach (var updated in info.objects.update)
                    ObjectUpdater.Instance.UpdateObject(updated);
                
                break;
            case "magicValid":
                MagicValidInfo magicValid = JsonUtility.FromJson<MagicValidInfo>(json);
                //vaildCheck
                if (magicValid.valid)
                {
                    foreach (var gameObject in CardInputSender.inputRequestDict[magicValid.id])
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    Debug.Log("유효한 움직임이 아닙니다!");
                }
                CardInputSender.inputRequestDict.Remove(magicValid.id);
                return;
            case "result":
                ResultInfo result = JsonUtility.FromJson<ResultInfo>(json);
                if (SceneContext.UserID == SceneContext.MatchInfo.leftUserId)
                {
                    SceneContext.MatchResult = result.leftPlayer;
                }
                else
                {
                    SceneContext.MatchResult = result.rightPlayer;
                }
                SceneManager.LoadScene("ResultScene");

                break;
        }
        
    }
}

using System;
using System.Collections;
using Global;
using UnityEngine;

namespace GameScene
{
    public class PingSender : MonoBehaviour
    {
        private const float PING_INTERVAL = 3f;
    
        private StompConnector stompConnector;
        private string destination;
        private string pingMessage;
        private Coroutine _pingCoroutine;

        [Serializable]
        class PingDto
        {
            public string type = "ping";
        }
    
        private void Awake()
        {
            pingMessage = JsonUtility.ToJson(new PingDto());
            stompConnector = FindObjectOfType<StompConnector>();
            destination = $"/app/game/input/{SceneContext.MatchInfo.sessionId}/{SceneContext.UserID}";
        }

        private void OnEnable()
        {
            _pingCoroutine = StartCoroutine(PingCoroutine());
        }
    
        private void OnDisable()
        {
            if (_pingCoroutine != null)
            {
                StopCoroutine(_pingCoroutine);
                _pingCoroutine = null;
            }
        }
    
        private IEnumerator PingCoroutine()
        {
            while (true)
            {
                stompConnector.SendMessageToServer(destination, pingMessage);
                yield return new WaitForSeconds(PING_INTERVAL);
            }
        }
    }
}

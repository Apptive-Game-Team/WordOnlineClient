using System;
using System.Collections;
using Scripts.Global;
using UnityEngine;

namespace Scripts.GameScene
{
    public class PingSender : MonoBehaviour
    {
        private const float PING_INTERVAL = 3f;
    
        private StompConnector stompConnector;
        private string destination;
        private string pingMessage;

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

        private void Start()
        {
            StartCoroutine(PingCoroutine());
        }
    
        private void OnDisable()
        {
            StopAllCoroutines();
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

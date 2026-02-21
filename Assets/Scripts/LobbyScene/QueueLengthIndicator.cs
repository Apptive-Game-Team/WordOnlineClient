using System;
using System.Collections;
using Data;
using Global;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;

namespace LobbyScene
{
    public class QueueLengthIndicator : MonoBehaviour
    {
        [SerializeField] private TMP_Text _queueLengthText;
        [SerializeField] private LocalizedString _queueLengthLocalizedString;
    
        [Serializable]
        private class QueueLengthResponse
        {
            public int length;
        }
    
        private void Start()
        {
            StartCoroutine(UpdateLengthPeriodically());
        }
    
        private IEnumerator UpdateLengthPeriodically()
        {
            while (true)
            {
                yield return FetchQueueLength();
                yield return new WaitForSeconds(11f);
            }
        }
    
        private IEnumerator FetchQueueLength()
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get($"{ServerList.MatchingServer.url}/api/match/length");
            Server.SetAuthorization(webRequest);
            Server.SetAcceptLanguage(webRequest);
            WDebug.Log("Fetching queue length from " + $"{ServerList.MatchingServer.url}/api/match/length");
            webRequest.downloadHandler = new DownloadHandlerBuffer();
        
            yield return webRequest.SendWebRequest();
        
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError($"Error fetching queue length: {webRequest.error}");
                yield break;
            }
        
            QueueLengthResponse response = JsonUtility.FromJson<QueueLengthResponse>(webRequest.downloadHandler.text);
        
            _queueLengthText.text = $"{_queueLengthLocalizedString.GetLocalizedString()} {response.length}";
        }
    }
}

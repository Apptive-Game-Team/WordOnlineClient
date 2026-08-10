using System;
using System.Collections;
using Data;
using Global;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Networking;
using Global.Serialization;

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
    
        // OnEnable, not Start: the matching page keeps its own indicator on a page
        // that is deactivated between matches, and polling has to resume on reopen.
        private void OnEnable()
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
        
            string body = webRequest.downloadHandler.text;
            if (!JsonCodec.TryDeserialize(body, out QueueLengthResponse response, out string error))
            {
                WDebug.LogError($"Error parsing queue length: {error} / {JsonCodec.Excerpt(body)}");
                yield break;
            }

            _queueLengthText.text = $"{_queueLengthLocalizedString.GetLocalizedString()} {response.length}";
        }
    }
}

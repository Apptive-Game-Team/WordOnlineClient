using System;
using System.Collections;
using System.Collections.Generic;
using Script.Global;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class QueueLengthIndicator : MonoBehaviour
{
    [SerializeField] private TMP_Text _queueLengthText;
    
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
            yield return new WaitForSeconds(30f);
        }
    }
    
    private IEnumerator FetchQueueLength()
    {
        using UnityWebRequest webRequest = UnityWebRequest.Get($"{SceneContext.CurrentServer.url}/api/match/length");
        webRequest.SetRequestHeader("Authorization", "Bearer " + SceneContext.JwtToken);
        WDebug.Log("Fetching queue length from " + $"{SceneContext.CurrentServer.url}/api/match/length" + " with token " + SceneContext.JwtToken);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        
        yield return webRequest.SendWebRequest();
        
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            WDebug.LogError($"Error fetching queue length: {webRequest.error}");
            yield break;
        }
        
        QueueLengthResponse response = JsonUtility.FromJson<QueueLengthResponse>(webRequest.downloadHandler.text);
        
        _queueLengthText.text = $"{response.length}";
    }
}

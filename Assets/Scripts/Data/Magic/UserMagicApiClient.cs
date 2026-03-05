using System;
using System.Collections;
using Global;
using UnityEngine;
using UnityEngine.Networking;

namespace Data.Magic
{
    public class UserMagicApiClient : MonoBehaviour
    {

        public void GetUserMagic(Action<UserMagicResponse> callback) {
            StartCoroutine(_GetUserMagic(callback));
        }
        
        private static IEnumerator _GetUserMagic(Action<UserMagicResponse> callback) {
            using UnityWebRequest webRequest = UnityWebRequest.Get($"{ServerList.MatchingServer.url}/api/users/mine/magics");
            
            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);

            webRequest.downloadHandler = new DownloadHandlerBuffer();
            
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                UserMagicResponse response = JsonUtility.FromJson<UserMagicResponse>(webRequest.downloadHandler.text);
            
                callback.Invoke(response);
            }
            else
            {
                WDebug.LogError($"Failed to get user magic: {webRequest.error}");
                callback.Invoke(null);
            }
        }
    }
}
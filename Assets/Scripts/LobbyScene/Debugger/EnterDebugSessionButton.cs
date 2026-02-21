using System;
using System.Collections;
using Scripts.Data;
using Scripts.Global;
using Scripts.Global.Button;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Scripts.LobbyScene.Debugger
{
    public class EnterDebugSessionButton : ButtonBase
    {

        [SerializeField] private string side;


        protected override void OnClickButton()
        {
            if (side.Equals("Practice"))
            {
                StartCoroutine(EnterPracticeDebugSession());
                return;
            }
            StartCoroutine(EnterDebugSession());
        }
        
        private IEnumerator EnterPracticeDebugSession()
        {
            using var request = UnityWebRequest.PostWwwForm($"http://localhost:7777/api/debug/game/practice", "");
            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError($"Error entering debug session: {request.error}");
                yield break;
            }

            String json = request.downloadHandler.text;
            
            WDebug.Log(json);
            
            DebugGameResponse response = JsonUtility.FromJson<DebugGameResponse>(json);

            SceneContext.MatchInfo = MatchedInfoDto.CreateDebugSession(response.sessionId, "left", SceneContext.UserID);
            SceneManager.LoadScene("GameScene");
        }
        

        private IEnumerator EnterDebugSession()
        {
            using var request = UnityWebRequest.PostWwwForm($"http://localhost:7777/api/debug/game/{side}", "");
            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError($"Error entering debug session: {request.error}");
                yield break;
            }

            String json = request.downloadHandler.text;
            
            WDebug.Log(json);
            
            DebugGameResponse response = JsonUtility.FromJson<DebugGameResponse>(json);

            SceneContext.MatchInfo = MatchedInfoDto.CreateDebugSession(response.sessionId, side, SceneContext.UserID);
            SceneManager.LoadScene("GameScene");
        }
    }
}

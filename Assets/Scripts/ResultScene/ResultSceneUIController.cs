using Data;
using GameScene.Dto;
using Global;
using TMPro;
using UnityEngine;

namespace ResultScene
{
    public class ResultSceneUIController : MonoBehaviour
    {
        public static ResultSceneUIController Instance;
        private ResultInfo resultInfo;

        [SerializeField] TextMeshProUGUI resultText;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // 결과 화면에 들어왔다는 것은 세션이 닫혔다는 뜻이다. 신고하지 않으면 로비 티켓이
            // MATCHED로 남아 다음 매칭이 조용히 무시된다. 결과 표시와는 독립적으로 진행한다.
            StartCoroutine(SessionLossReporter.ReportMatchCompleted());

            resultInfo = SceneContext.MatchResult;
            SceneContext.MatchResult = null;

            if (resultInfo == null)
            {
                WDebug.LogError("ResultInfo is null. Cannot display result.");
                SetResultText("No result available.");
                return;
            }
        
            if (SceneContext.Me == "LeftPlayer")
            {
                SetResultText($"{resultInfo.leftPlayer}\n" +
                              $"MMR: {resultInfo.lastLeftPlayerMmr} -> {resultInfo.newLeftPlayerMmr}");
            }
            else if (SceneContext.Me == "RightPlayer")
            {
                SetResultText($"{resultInfo.rightPlayer}\n" +
                              $"MMR: {resultInfo.lastRightPlayerMmr} -> {resultInfo.newRightPlayerMmr}");
            }
            else
            {
                SetResultText("You are not part of this match.");
            }
        }

        private void SetResultText(string text)
        {
            resultText.text = text;
        }
    }
}

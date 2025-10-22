using Script.Global;
using TMPro;
using UnityEngine;

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

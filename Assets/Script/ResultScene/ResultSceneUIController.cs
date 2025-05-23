using System;
using System.Collections;
using System.Collections.Generic;
using Script.GameScene;
using TMPro;
using UnityEngine;

public class ResultSceneUIController : MonoBehaviour
{
    public static ResultSceneUIController Instance;

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
        SetResultText(SceneContext.MatchResult);
    }

    private void SetResultText(string text)
    {
        resultText.text = text;
    }
}

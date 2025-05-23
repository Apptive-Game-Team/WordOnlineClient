using System;
using System.Collections;
using System.Collections.Generic;
using Script.GameScene;
using TMPro;
using UnityEngine;

public class GameSceneUIController : MonoBehaviour
{
    public static GameSceneUIController Instance;
    
    [SerializeField] private TextMeshProUGUI leftUserIDText;
    [SerializeField] private TextMeshProUGUI rightUserIDText;
    [SerializeField] private TextMeshProUGUI manaText;
    
    [SerializeField] private CardUI cardUIPrefab;
    [SerializeField] private GameObject lowerBar;

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
#if UNITY_WEBGL && !UNITY_EDITOR
        leftUserIDText.text = SceneContext.MatchInfo.leftUserId;
        rightUserIDText.text = SceneContext.MatchInfo.rightUserId;
#endif

    }

    public void UpdateMana(int mana)
    {
        manaText.text = mana.ToString();
    }

    public void AddCard(string cardname)
    {
        CardUI cardUI = Instantiate(cardUIPrefab, lowerBar.transform);
        cardUI.Init(cardname);
    }
}

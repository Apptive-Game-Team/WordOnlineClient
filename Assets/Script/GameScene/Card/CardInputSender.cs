using System;
using System.Collections.Generic;
using Script.Data;
using Script.GameScene;
using Script.GameScene.Card;
using Script.Global;
using Script.Global.Util;
using UnityEngine;

public class CardInputSender : MonoBehaviour, ICardSender
{
    public static Dictionary<int,List<CardUI>> inputRequestDict = new Dictionary<int, List<CardUI>>();
    private List<string> _currentCardNameList = new List<string>();
    private List<CardUI> _currentCardList = new List<CardUI>();
    
    public bool CanSelectField => _currentCardList.Count >= 1;
    private bool isFieldSelectMode = false;

    public bool IsFieldSelectMode()
    {
        return isFieldSelectMode;    
    }

    public void CancelUseCard(CardUI cardObj)
    {
        WDebug.Log($"CancelUseCard: {cardObj.CardName}");
        if (_currentCardNameList.Contains(cardObj.CardName))
        {
            _currentCardNameList.Remove(cardObj.CardName);
            _currentCardList.Remove(cardObj);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Cancel();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Confirm();
        }
    }

    public void Cancel()
    {
        if (isFieldSelectMode)
        {
            CancelAll();
        }
    }
    
    public void Confirm()
    {
        if (CanSelectField)
        {
            if (!CombinedMagicResolver.CanResolve(GetCurrentRecipeTypes()))
            {
                WDebug.Log("Cannot resolve the current recipe.");
                PlayerFeedbackController.Instance.UseMagicFeedback();
                SendInput(Vector2.zero);
                return;
            }
            
            isFieldSelectMode = true;
        }
    }
    

    public string GetMagicName()
    {
        string result =
            _currentCardNameList.Find(c => c.Contains("Build")) ??
            _currentCardNameList.Find(c => c.Contains("Spawn")) ??
            _currentCardNameList.Find(c => c.Contains("Explode")) ??
            _currentCardNameList.Find(c => c.Contains("Drop")) ??
            _currentCardNameList.Find(c => c.Contains("Shoot"));
        return result;
    }
    
    private void CancelAll()
    {
        WDebug.Log("CancelAll");
        foreach (var card in _currentCardList)
        {
            card.SetCardActive(false);
        }
        _currentCardList.Clear();
        _currentCardNameList.Clear();
        PlayerFeedbackController.Instance.UseMagicFeedback();
        FindObjectOfType<CardInputSender>().SetExpectedMagicUI(); 
        isFieldSelectMode = false;
    }

    public void TryUseCard(CardUI cardObj)
    {
        AddCardList(cardObj);
    }

    public void SendInput(Vector3 pos) //whenFieldSelect
    {
        var input = new CardUseInput(new List<string>(_currentCardNameList), pos);
        string json = JsonUtility.ToJson(input);
        
        string destination = $"/app/game/input/{SceneContext.MatchInfo.sessionId}/{SceneContext.UserID}";
        StompConnector.Instance.SendMessageToServer(destination, json);
        inputRequestDict.Add(input.id, new List<CardUI>(_currentCardList)) ;
        _currentCardNameList.Clear();
        _currentCardList.Clear();
        isFieldSelectMode = false;
    }
    
    private void AddCardList(CardUI card)
    {
        WDebug.Log("AddCardList: " + card.CardName);
        _currentCardNameList.Add(card.CardName);
        _currentCardList.Add(card);
    }

    private List<CardType> GetCurrentRecipeTypes()
    {
        var list = new List<CardType>(_currentCardList.Count);
        foreach (var c in _currentCardList)
        {
            if (CardNameMapper.TryMapToCardType(c.CardName, out var t))
                list.Add(t);
            else
                WDebug.LogWarning($"[CardInputSender] Unknown CardName → CardType map: {c.CardName}");
        }
        return list;
    }
    public void SetExpectedMagicUI()
    {
        GameSceneUIController.Instance.TrySetExpectedMagicUI(GetCurrentRecipeTypes());
    }
}
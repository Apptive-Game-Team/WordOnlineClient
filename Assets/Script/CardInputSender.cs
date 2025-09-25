using System;
using System.Collections.Generic;
using Script.Data;
using Script.GameScene;
using UnityEngine;

public class CardInputSender : MonoBehaviour
{
    public static Dictionary<int,List<CardUI>> inputRequestDict = new Dictionary<int, List<CardUI>>();
    private List<string> _currentCardNameList = new List<string>();
    private List<CardUI> _currentCardList = new List<CardUI>();
    
    public bool CanSelectField => _currentCardList.Count >= 2;
    public bool IsFieldSelectMode { get; private set; } = false;

    public void CancelUseCard(CardUI cardObj)
    {
        Debug.Log($"CancelUseCard: {cardObj.CardName}");
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
        if (IsFieldSelectMode)
        {
            CancelAll();
        }
    }
    
    public void Confirm()
    {
        if (CanSelectField)
        {
            IsFieldSelectMode = true;
        }
    }
    

    public string GetMagicName()
    {
        string result =
            _currentCardNameList.Find(c => c.Contains("Build")) ??
            _currentCardNameList.Find(c => c.Contains("Spawn")) ??
            _currentCardNameList.Find(c => c.Contains("Explode")) ??
            _currentCardNameList.Find(c => c.Contains("Shoot"));
        return result;
    }
    
    private void CancelAll()
    {
        Debug.Log("CancelAll");
        foreach (var card in _currentCardList)
        {
            card.SetCardActive(false);
        }
        _currentCardList.Clear();
        _currentCardNameList.Clear();
        PlayerFeedbackController.Instance.UseMagicFeedback();
        FindObjectOfType<CardInputSender>().SetExpectedMagicUI(); 
        IsFieldSelectMode = false;
    }

    public void TryUseCard(CardUI cardObj)
    {
        AddCardList(cardObj);
    }

    public void SendInput(Vector2 pos) //whenFieldSelect
    {
        var input = new CardUseInput(new List<string>(_currentCardNameList), pos);
        string json = JsonUtility.ToJson(input);
        
        string destination = $"/app/game/input/{SceneContext.MatchInfo.sessionId}/{SceneContext.UserID}";
        StompConnector.Instance.SendMessageToServer(destination, json);
        inputRequestDict.Add(input.id, new List<CardUI>(_currentCardList)) ;
        _currentCardNameList.Clear();
        _currentCardList.Clear();
        IsFieldSelectMode = false;
    }
    
    
    public void AddCardList(CardUI card)
    {
        Debug.Log("AddCardList: " + card.CardName);
        _currentCardNameList.Add(card.CardName);
        _currentCardList.Add(card);
    }
    
    private static readonly Dictionary<string, CardType> NameToType = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Build", CardType.Build }, { "Spawn", CardType.Spawn }, { "Shoot", CardType.Shoot }, { "Explode", CardType.Explode },
        { "Fire", CardType.Fire }, { "Water", CardType.Water }, { "Lightning", CardType.Lightning },
        { "Nature", CardType.Nature }, { "Rock", CardType.Rock }, { "Wind", CardType.Wind },
    };

    private bool TryMapToCardType(string name, out CardType type)
    {
        if (Enum.TryParse(name, true, out type)) return true;
        return NameToType.TryGetValue(name, out type);
    }

    private List<CardType> GetCurrentRecipeTypes()
    {
        var list = new List<CardType>(_currentCardList.Count);
        foreach (var c in _currentCardList)
        {
            if (TryMapToCardType(c.CardName, out var t))
                list.Add(t);
            else
                Debug.LogWarning($"[CardInputSender] Unknown CardName → CardType map: {c.CardName}");
        }
        return list;
    }
    public void SetExpectedMagicUI()
    {
        GameSceneUIController.Instance.TrySetExpectedMagicUI(GetCurrentRecipeTypes());
    }
}
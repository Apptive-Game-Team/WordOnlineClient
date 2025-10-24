using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class MatchingStatusTextUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    private static MatchingStatusTextUI Instance;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        text.text = "";
    }
    
    public static void SetMatchingStatusText(LocalizedString localizedString)
    {
        if (Instance != null)
        {
            localizedString.GetLocalizedStringAsync().Completed += handle =>
            {
                Instance.text.text = handle.Result;
            };
        }
    }

    public static void SetMatchingStatusText(string status)
    {
        if (Instance != null)
        {
            Instance.text.text = status;
        }
    }
}

using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SystemMessageUI : MonoBehaviour
{
    public static SystemMessageUI Instance;

    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float duration = 5f; 
    [SerializeField] private float replaceDelay = 1f; 

    private string currentMessage = "";
    private Coroutine displayRoutine;
    private Coroutine replaceRoutine;

    void Awake()
    {
        Instance = this;
        messageText.gameObject.SetActive(false);
    }

    public void ShowMessage(string msg)
    {
        if (msg == currentMessage) return;
        
        if (displayRoutine != null)
        {
            if (replaceRoutine != null) StopCoroutine(replaceRoutine);
            replaceRoutine = StartCoroutine(ReplaceAfterDelay(msg));
        }
        else
        {
            displayRoutine = StartCoroutine(DisplayMessage(msg));
        }
    }

    private IEnumerator ReplaceAfterDelay(string msg)
    {
        yield return new WaitForSeconds(replaceDelay);
        StopCoroutine(displayRoutine);
        displayRoutine = StartCoroutine(DisplayMessage(msg));
    }

    private IEnumerator DisplayMessage(string msg)
    {
        currentMessage = msg;
        messageText.text = msg;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        messageText.gameObject.SetActive(false);
        currentMessage = "";
        displayRoutine = null;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Scripts.Global
{
    public class SystemMessageUI : LocalSingletonObject<SystemMessageUI>
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private float duration = 5f; 
        [SerializeField] private float replaceDelay = 1f; 

        private static readonly Queue<string> messageQueue = new Queue<string>();
        private Coroutine messageRoutine;

        protected override void Awake()
        {
            base.Awake();
            if (messagePanel == null)
                messagePanel = messageText.transform.parent.gameObject;
            
            messagePanel.SetActive(false);
        }

        private void Start()
        {
            if (messageQueue.Count > 0 && messageRoutine == null)
            {
                messageRoutine = StartCoroutine(ProcessMessageQueue());
            }
        }

        public void ShowMessage(LocalizedString localizedString, Action callback = null)
        {
            localizedString.GetLocalizedStringAsync().Completed += handle =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    ShowMessage(handle.Result);
                }
                callback?.Invoke();
            };
        }
    
        public void ShowMessage(string msg)
        {
            messageQueue.Enqueue(msg);

            if (messageRoutine == null)
            {
                messageRoutine = StartCoroutine(ProcessMessageQueue());
            }
        }

        private IEnumerator ProcessMessageQueue()
        {
            while (messageQueue.Count > 0)
            {
                string msg = messageQueue.Peek();
            
                messageText.text = msg;
                messagePanel.SetActive(true);

                float timer = 0f;
                float currentDuration = duration;

                while (timer < currentDuration)
                {
                    timer += Time.deltaTime;
                
                    if (messageQueue.Count > 1 && timer >= replaceDelay)
                    {
                        break; 
                    }
                
                    yield return null;
                }
            
                messageQueue.Dequeue();
            }

            messagePanel.SetActive(false);
            messageText.text = "";
            messageRoutine = null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using Global;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TutorialScene
{
    public class TutorialDisplay : MonoBehaviour
    {
        [SerializeField] private List<GameObject> tutorialPanel;
    
        private int currentIndex = 0;
    
        private void Start()
        {
            if (tutorialPanel.Count == 0)
            {
                WDebug.LogWarning("No tutorial panels assigned.");
                return;
            }
        
            ShowPanel(0);
            StartCoroutine(TutorialCoroutine());
        }

        private IEnumerator TutorialCoroutine()
        {
            currentIndex = 0;
            while (currentIndex < tutorialPanel.Count)
            {
                WDebug.Log(currentIndex + 1 + " / " + tutorialPanel.Count);
                ShowPanel(currentIndex);
                currentIndex++;
                yield return new WaitForSeconds(0.5f);
                yield return new WaitUntil(() => Input.anyKeyDown);
            }
            OnEnd();
        }
    
        private void ShowPanel(int index)
        {
            for (int i = 0; i < tutorialPanel.Count; i++)
            {
                tutorialPanel[i].SetActive(i == index);
            }
        }


        private void OnEnd()
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }
}

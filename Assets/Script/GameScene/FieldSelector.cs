using System;
using System.Collections;
using System.Collections.Generic;
using Script.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FieldSelector : MonoBehaviour
{
    CardInputSender cardInputSender;
    private GameObject currentAimObj;
    private GameObject currentRangeObj;
    private GameObject currentSkillIndicator;
    [SerializeField] private GameObject aimObject;
    [SerializeField] private GameObject rangeObject;
    [SerializeField] private GameObject lineSkillIndicator;
    [SerializeField] private GameObject circleSkillIndicator;
    private void Start()
    {
        cardInputSender = FindObjectOfType<CardInputSender>();
        currentAimObj = Instantiate(aimObject);
        currentRangeObj = Instantiate(rangeObject);
        currentSkillIndicator = Instantiate(circleSkillIndicator);
    }

    void Update()
    {
        if (cardInputSender.IsFieldSelectMode)
        {
            if (!currentAimObj.activeSelf)
            {
                currentAimObj.SetActive(true);
            }
            currentRangeObj.SetActive(true);
            currentRangeObj.transform.localScale = Vector3.one * (40f * LocalMagicData.GetMagicData(cardInputSender.GetMagicName()).range);
            if (LocalMagicData.GetMagicData(cardInputSender.GetMagicName()).name.Equals("Shoot"))
            {
                if (currentSkillIndicator.name.Contains("circle"))
                {
                    Destroy(currentSkillIndicator);
                    currentSkillIndicator = Instantiate(lineSkillIndicator);
                }
                else
                {
                    currentSkillIndicator.SetActive(true);
                }
            }
            else
            {
                if (currentSkillIndicator.name.Contains("line"))
                {
                    Destroy(currentSkillIndicator);
                    currentSkillIndicator = Instantiate(circleSkillIndicator);
                }
                else
                {
                    currentSkillIndicator.SetActive(true);
                }
            }
            if (SceneContext.Me.Equals("RightPlayer"))
            {
                currentRangeObj.transform.position = new Vector3(17, 5, 0);
            }
            else
            {
                currentRangeObj.transform.position = new Vector3(1, 5, 0);
            }            
            
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentAimObj.transform.position = mouseWorldPos;



            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                cardInputSender.SendInput(mouseWorldPos);
                PlayerFeedbackController.Instance.UseMagicFeedback();
                currentAimObj.SetActive(false);
                currentRangeObj.SetActive(false);
                currentSkillIndicator.SetActive(false);
                cardInputSender.SetExpectedMagicUI(); 
            }
        } else {
            if (currentAimObj.activeSelf)
            {
                currentAimObj.SetActive(false);
                currentRangeObj.SetActive(false);
                currentSkillIndicator.SetActive(false);
            }
        }
        

    }
}

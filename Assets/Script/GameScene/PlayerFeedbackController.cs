using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerFeedbackController : MonoBehaviour
{
    
    public static PlayerFeedbackController Instance;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    private GameObject playerObject;

    public GameObject PlayerObject
    {
        get
        {
            if (playerObject == null)
            {
                playerObject = GetPlayerObject();
            }
            return playerObject;
        }
    }
    
    private GameObject GetPlayerObject()
    {
        if (SceneContext.Me.Equals("LeftPlayer"))
        {
            return GameObject.Find("LeftPlayer");
        }
        else if (SceneContext.Me.Equals("RightPlayer"))
        {
            return GameObject.Find("RightPlayer");
        }

        return null;
    }

    public void PlayCardSelectFeedback()
    {
        Transform spriteTr = PlayerObject.transform.Find("PlayerSprite");
        DOTweenAction.RotatePlayerUseCard(spriteTr);
    }
    public void CancelCardSelectFeedback()
    {
        Transform spriteTr = PlayerObject.transform.Find("PlayerSprite");
        DOTweenAction.RotatePlayerCancelCard(spriteTr);
    }

    public void UseMagicFeedback()
    {
        Transform spriteTr = PlayerObject.transform.Find("PlayerSprite");
        DOTweenAction.RotatePlayerUseMagic(spriteTr);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerFeedbackController : MonoBehaviour
{
    private GameObject playerObject;

    public GameObject PlayerObject
    {
        get
        {
            if (playerObject == null)
            {
                GetPlayerObject();
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
        DOTweenAction.SwingPlayerUseCard(spriteTr);
    }
}

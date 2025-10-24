using System;
using System.Collections;
using System.Collections.Generic;
using Script.Data;
using UnityEngine;
using Script.GameScene;
using Script.GameScene.Dto; // ServedObject, ObjectContainer 등
using Script.GameScene.Exception;
using Script.GameScene.Object;
using Script.Global; // DuplicatedException
using UnityEngine.UI;

public class RejoinSyncHelper : MonoBehaviour
{
    private void Start()
    {
        if (!string.IsNullOrEmpty(SceneContext.RejoinSyncJson))
            StartCoroutine(ApplySnapshotJson(SceneContext.RejoinSyncJson));
    }
    
    private IEnumerator ApplySnapshotJson(string json)
    {
        yield return null;
    
        SnapshotDto dto = null;
        try
        {
            dto = JsonUtility.FromJson<SnapshotDto>(json);
        }
        catch (Exception e)
        {
            WDebug.LogError($"[Rejoin] Snapshot JSON parse error: {e}\n{json}");
            yield break;
        }
        
        if (dto.objects != null)
        {
            foreach (var o in dto.objects)
            {
                if(o.prefab != "Player") SpawnObject(o);
            }
        }
        
        if (dto.myCards != null)
        {
            foreach (var cardName in dto.myCards)
            {
                GameSceneUIController.Instance.AddCard(cardName);
            }
            
        }
    
        SceneContext.RejoinSyncJson = null;
        SystemMessageUI.Instance.ShowMessage("이전 게임 세션에 재접속했습니다.");
    }
    
    private void SpawnObject(SnapshotObjectDto o)
    {
        CreatedObjectDto createdObjectDto = new CreatedObjectDto(o);
        ObjectSpawner.Instance.SpawnObject(createdObjectDto);
    }
}

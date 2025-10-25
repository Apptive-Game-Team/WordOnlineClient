using System;
using System.Collections;
using UnityEngine;
using Script.GameScene.Dto; // ServedObject, ObjectContainer 등
using Script.GameScene.Object;
using Script.Global;
using UnityEngine.Localization; // DuplicatedException

public class RejoinSyncHelper : MonoBehaviour
{
    
    public LocalizedString rejoinSuccessMessage;
    
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
        SystemMessageUI.Instance.ShowMessage(rejoinSuccessMessage);
    }
    
    private void SpawnObject(SnapshotObjectDto o)
    {
        CreatedObjectDto createdObjectDto = new CreatedObjectDto(o);
        ObjectSpawner.Instance.SpawnObject(createdObjectDto);
    }
}

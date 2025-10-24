using System;
using System.Collections;
using System.Collections.Generic;
using Script.Data;
using UnityEngine;
using Script.GameScene;              // ServedObject, ObjectContainer 등
using Script.GameScene.Exception;
using Script.Global;
using UnityEngine.Localization; // DuplicatedException
using UnityEngine.UI;

public class RejoinSyncHelper : MonoBehaviour
{
    [Serializable]
    private class SnapshotDto
    {
        public int frame;
        public SnapshotObjectDto[] objects;
        public string[] myCards;
    }

    [Serializable]
    private class SnapshotObjectDto
    {
        public int id;
        public string prefab;
        public float x;
        public float y;
        public float z;
        public string master;
        public string status;
        public string effect;
    }

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
        CreatedObjectDto createdObjectDto = new CreatedObjectDto()
        {
            id = o.id,
            master = o.master,
            position = new Vector3(o.x, o.y, o.z),
            type = o.prefab
        };
        ObjectSpawner.Instance.SpawnObject(createdObjectDto);
    }
}

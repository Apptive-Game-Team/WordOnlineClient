using System.Collections.Generic;
using Script.GameScene.Dto;
using UnityEngine;
using System.Linq;
using Script.Global;

namespace Script.GameScene.Object
{
    public class ObjectSyncer : MonoBehaviour
    {
        public static ObjectSyncer Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Sync(SnapshotObjectDto[] snapshotObjects)
        {
            WDebug.Log("ObjectSyncer Sync called with " + string.Join(" | ", snapshotObjects.Select(dto => $"id: {dto.id}, {dto.prefab}")) + " objects.");
            foreach (var snapshotObject in snapshotObjects)
            {
                if (!ObjectContainer.Instance.IsExist(snapshotObject.id))
                {
                    // create
                    ObjectSpawner.Instance.SpawnObject(new CreatedObjectDto(snapshotObject));
                }
          
                // update
                ObjectUpdater.Instance.UpdateObject(new UpdatedObjectDto(snapshotObject));
            }

            List<int> ids = snapshotObjects.Select(snapshotObject => snapshotObject.id).ToList();
            foreach (var i in ObjectContainer.Instance.GetIds()
                         .Where(id => !ids.Contains(id)))
            {
                ObjectContainer.Instance.FindById(i)
                    .DestroySelf();
            }
        }
    }
}
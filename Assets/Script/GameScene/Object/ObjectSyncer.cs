using System.Collections.Generic;
using Script.GameScene.Dto;
using UnityEngine;
using System.Linq;
using Script.Global;

namespace Script.GameScene.Object
{
    public class ObjectSyncer : LocalSingletonObject<ObjectSyncer>
    {

        public void Sync(SnapshotObjectDto[] snapshotObjects)
        {
            // WDebug.Log("ObjectSyncer Sync called with " + string.Join(" | ", snapshotObjects.Select(dto => $"id: {dto.id}, {dto.prefab}")) + " objects.");
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
            List<int> toRemove = ObjectContainer.Instance.GetIds()
                .Where(id => !ids.Contains(id))
                .ToList();
            foreach (var i in toRemove)
            {
                ObjectContainer.Instance.UnregisterObject(i);
            }
        }
    }
}
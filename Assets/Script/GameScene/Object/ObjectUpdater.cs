using Script.Global;
using UnityEngine;

namespace Script.GameScene.Object
{
    public class ObjectUpdater : LocalSingletonObject<ObjectUpdater>
    {
        
        public void UpdateObject(UpdatedObjectDto updatedObjectDto)
        {
            // WDebug.Log("ObjectUpdater UpdateObject called for ID: " + updatedObjectDto.id);
            ServedObject servedObject = ObjectContainer.Instance.FindById(updatedObjectDto.id);
            if (servedObject != null)
            {
                servedObject.UpdateObject(updatedObjectDto);
            }
            // TODO - Add Logic for Animation, State, Effect Atc
        }
    }
}
using System.Collections.Generic;
using Script.GameScene.Exception;
using Script.Global;
using System.Linq;

namespace Script.GameScene.Object
{
    public class ObjectContainer : LocalSingletonObject<ObjectContainer>
    {
        
        private readonly Dictionary<int, ServedObject> objects = new Dictionary<int, ServedObject>();
        
        public void RegisterObject(ServedObject obj)
        {
            if (!obj)
            {
                throw new System.ArgumentNullException(nameof(obj), "Object or ID cannot be null or empty.");
            }
            
            
            if (!objects.TryAdd(obj.id, obj))
            {
                WDebug.Log("Attempted to register duplicate object with ID: " + obj.id + $" already exists. {objects[obj.id].id},  {objects[obj.id]}");
                throw new DuplicatedException($"Object with ID {obj.id} already exists.");
            }
            
            WDebug.Log("Registered object with ID: " + obj.id);
        }
        
        public void UnregisterObject(int id)
        {
            ServedObject servedObject = FindById(id);
            Destroy(servedObject);
            UnregisterObject(servedObject);
        }
        
        public void UnregisterObject(ServedObject obj)
        {
            if (obj == null)
            {
                throw new System.ArgumentNullException(nameof(obj), "Object or ID cannot be null or empty.");
            }
            
            if (!objects.Remove(obj.id))
            {
                WDebug.LogWarning($"Object with ID {obj.id} not found in container.");
            }
        }
        
        public bool IsExist(int id)
        {
            return objects.ContainsKey(id);
        }
        
        public ServedObject FindById(int id)
        {
            return objects.GetValueOrDefault(id, null);
        }

        public List<int> GetIds()
        {
            return objects.Keys.ToList();
        }
    }
}
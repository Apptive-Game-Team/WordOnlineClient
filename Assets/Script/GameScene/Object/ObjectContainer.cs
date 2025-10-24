using System.Collections.Generic;
using Script.GameScene.Exception;
using Script.Global;
using UnityEngine;
using System.Linq;

namespace Script.GameScene.Object
{
    public class ObjectContainer : MonoBehaviour
    {
        public static ObjectContainer Instance { get; private set; }
        
        private Dictionary<int, ServedObject> objects = new Dictionary<int, ServedObject>();
        
        private void Awake()
        {
            Instance = this;
        }
        
        public void RegisterObject(ServedObject obj)
        {
            if (obj == null)
            {
                throw new System.ArgumentNullException(nameof(obj), "Object or ID cannot be null or empty.");
            }
            
            if (objects.ContainsKey(obj.id))
            {
                throw new DuplicatedException($"Object with ID {obj.id} already exists.");
            }
            
            objects[obj.id] = obj;
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
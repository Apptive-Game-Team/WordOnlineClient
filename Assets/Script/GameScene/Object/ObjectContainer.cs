using System.Collections.Generic;
using Script.GameScene.Exception;
using Script.Global;
using System.Linq;
using UnityEngine;

namespace Script.GameScene.Object
{
    public class ObjectContainer : LocalSingletonObject<ObjectContainer>
    {
        private readonly Dictionary<int, ServedObject> objects = new Dictionary<int, ServedObject>();

        public void RegisterObject(ServedObject obj)
        {
            if (obj == null)
            {
                throw new System.ArgumentNullException(nameof(obj), "ServedObject to register cannot be null.");
            }

            if (!objects.TryAdd(obj.id, obj))
            {
                WDebug.LogWarning($"Object with ID {obj.id} already exists in the container. Ignoring registration.");
                throw new DuplicatedException($"Object with ID {obj.id} already exists.");
            }
            WDebug.Log("Registered object with ID: " + obj.id);
        }

        public void UnregisterObject(int id)
        {
            if (objects.Remove(id, out ServedObject servedObject))
            {
                if (servedObject != null && servedObject.gameObject != null)
                {
                    Destroy(servedObject.gameObject);
                    WDebug.Log($"Unregistered and destroyed object with ID: {id}");
                }
                else
                {
                    WDebug.Log($"Unregistered object with ID: {id}. GameObject was already destroyed or null.");
                }
            }
            else
            {
                WDebug.LogWarning($"Attempted to unregister an object with ID {id} that was not in the container.");
            }
        }

        public bool IsExist(int id)
        {
            return objects.ContainsKey(id);
        }

        public ServedObject FindById(int id)
        {
            objects.TryGetValue(id, out ServedObject obj);
            return obj;
        }

        public List<int> GetIds()
        {
            return objects.Keys.ToList();
        }
    }
}
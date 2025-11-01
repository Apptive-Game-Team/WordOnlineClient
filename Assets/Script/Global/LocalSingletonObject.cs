using UnityEngine;

namespace Script.Global
{
    public abstract class LocalSingletonObject<T> : MonoBehaviour where T : LocalSingletonObject<T>
    {
        
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            Instance = this as T;
        }
    }
}
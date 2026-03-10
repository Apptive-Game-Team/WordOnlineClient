using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Global
{
    public abstract class BoundedSingletonObject<T> : SingletonObject<T> where T : SingletonObject<T>
    {
        protected abstract List<string> BoundScenes { get; }
        
        protected override void Awake()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (!BoundScenes.Contains(currentSceneName))
            {
                Destroy(gameObject);
                return;
            }
            
            base.Awake();
        }
        
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!BoundScenes.Contains(scene.name))
            {
                Destroy(gameObject);
            }
        }
    }
}
using Script.GameScene.Exception;
using UnityEngine;

namespace Script.GameScene
{
    public class ObjectSpawner : MonoBehaviour
    {
        public static ObjectSpawner Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        
        public void SpawnObject(CreatedObjectDto createdObjectDto)
        {
            Vector3 position = new Vector3(
                createdObjectDto.position.x, 
                createdObjectDto.position.y, 
                createdObjectDto.position.z);
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{createdObjectDto.type}");
            GameObject spawnedObject = Instantiate(prefab, position, prefab.transform.rotation);
            ServedObject servedObject = spawnedObject.AddComponent<ServedObject>();
            servedObject.SetMaster(createdObjectDto.master);

            switch (createdObjectDto.master)
            {
                default:
                    Debug.LogWarning($"Unknown master: {createdObjectDto.master}");
                    break;
            }

            if (createdObjectDto.type.Contains("Slime"))
            {
                DOTweenAction.CrawlMob(spawnedObject.transform.GetChild(0));
            }
            
            servedObject.id = createdObjectDto.id;
            try
            {
                ObjectContainer.Instance.RegisterObject(servedObject);
            } catch (DuplicatedException e)
            {
                Debug.LogError($"Failed to register object: {e.Message}");
                Destroy(spawnedObject);
            }
        }
    }
}
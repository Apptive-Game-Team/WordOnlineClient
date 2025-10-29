using Script.Data.Sound;
using Script.GameScene.Exception;
using Script.Global;
using UnityEngine;

namespace Script.GameScene.Object
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
            WDebug.Log($"Spawning object: {createdObjectDto.type}, id: {createdObjectDto.id}");
            Vector3 position = new Vector3(
                createdObjectDto.position.x, 
                createdObjectDto.position.y, 
                createdObjectDto.position.z);
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{createdObjectDto.type}");
            GameObject spawnedObject = Instantiate(prefab, position, prefab.transform.rotation);
            ServedObject servedObject = spawnedObject.AddComponent<ServedObject>();
            AudioSource[] audioSource = spawnedObject.GetComponentsInChildren<AudioSource>();
            if (audioSource.Length > 0)
            {
                foreach (var source in audioSource)
                {
                    source.volume = SoundData.gameVolume / 100f;
                }
            }
            
            servedObject.SetMaster(createdObjectDto.master);

            switch (createdObjectDto.master)
            {
                default:
                    WDebug.LogWarning($"Unknown master: {createdObjectDto.master}");
                    break;
            }
            
            servedObject.id = createdObjectDto.id;
            try
            {
                ObjectContainer.Instance.RegisterObject(servedObject);
            } catch (DuplicatedException e)
            {
                WDebug.LogError($"Failed to register object: {e.Message}");
                Destroy(spawnedObject);
            }
        }
    }
}
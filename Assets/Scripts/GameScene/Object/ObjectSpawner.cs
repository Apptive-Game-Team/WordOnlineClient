using Data;
using Data.Sound;
using GameScene.Exception;
using GameScene.ServedObjectComponent;
using Global;
using Unity.VisualScripting;
using UnityEngine;

namespace GameScene.Object
{
    public class ObjectSpawner : LocalSingletonObject<ObjectSpawner>
    {
        
        public void SpawnObject(CreatedObjectDto createdObjectDto)
        {
            if (ObjectContainer.Instance.IsExist(createdObjectDto.id))
            {
                WDebug.LogWarning($"Object with ID {createdObjectDto.id} already exists. Spawn aborted.");
                return;
            }
            
            WDebug.Log($"Spawning object: {createdObjectDto.type}, id: {createdObjectDto.id}");
           
            GameObject spawnedObject = InstantiateGameObject(createdObjectDto);
            
            ServedObject servedObject = spawnedObject.GetOrAddComponent<ServedObject>();
            
            SetAudioSourceVolume(spawnedObject);
            
            servedObject.SetMaster(createdObjectDto.master);
            servedObject.id = createdObjectDto.id;
            
            WDebug.Log($"Spawned object: {spawnedObject}, master set to: {createdObjectDto.master}, id set to: {createdObjectDto.id}");
            try
            {
                ObjectContainer.Instance.RegisterObject(servedObject);
            } catch (DuplicatedException e)
            {
                WDebug.LogError($"Failed to register object: {e.Message}");
                Destroy(spawnedObject);
            }
        }
        
        private void SetAudioSourceVolume(GameObject obj)
        {
            AudioSource[] audioSources = obj.GetComponentsInChildren<AudioSource>();
            foreach (var source in audioSources)
            {
                source.volume = source.volume * SoundData.gameVolume / 100f;
            }
            WDebug.Log($"Spawned object: {obj}, audio sources set: {audioSources.Length}");
        }

        private GameObject InstantiateGameObject(CreatedObjectDto createdObjectDto)
        {
            GameObject spawnedObject;
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{createdObjectDto.type}");
            
            WDebug.Log($"Spawning object: {createdObjectDto.type}, prefab found: {prefab != null}");
            
            if (!prefab)
            {
                spawnedObject = new GameObject(createdObjectDto.type);
                spawnedObject.transform.position = createdObjectDto.position;
            }
            else 
            {
                spawnedObject = Instantiate(prefab, createdObjectDto.position, prefab.transform.rotation);
            }
            
            WDebug.Log($"Spawned object: {spawnedObject}, gameObject created at position {createdObjectDto.position}");
            return spawnedObject;
        }
    }
}
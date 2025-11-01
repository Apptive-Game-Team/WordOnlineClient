using Script.Data.Sound;
using Script.GameScene.Exception;
using Script.Global;
using UnityEngine;

namespace Script.GameScene.Object
{
    public class ObjectSpawner : LocalSingletonObject<ObjectSpawner>
    {
        
        // private void Start()
        // {
        //     SpawnObject(new CreatedObjectDto
        //     {
        //         id = 0,
        //         master = "None",
        //         position = new Vector3(3, 3, 0),
        //         type = "Player"
        //     });
        // }
        public void SpawnObject(CreatedObjectDto createdObjectDto)
        {
            WDebug.Log($"Spawning object: {createdObjectDto.type}, id: {createdObjectDto.id}");
            Vector3 position = new Vector3(
                createdObjectDto.position.x, 
                createdObjectDto.position.y, 
                createdObjectDto.position.z);
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{createdObjectDto.type}");
            GameObject spawnedObject;
            
            WDebug.Log($"Spawning object: {createdObjectDto.type}, prefab found: {prefab != null}");
            
            if (!prefab)
            {
                spawnedObject = new GameObject(createdObjectDto.type);
                spawnedObject.transform.position = position;
            }
            else 
            {
                spawnedObject = Instantiate(prefab, position, prefab.transform.rotation);
            }
            
            WDebug.Log($"Spawned object: {spawnedObject}, gameObject created at position {position}");
            
            ServedObject servedObject = spawnedObject.AddComponent<ServedObject>();
            AudioSource[] audioSource = spawnedObject.GetComponentsInChildren<AudioSource>();
            if (audioSource.Length > 0)
            {
                foreach (var source in audioSource)
                {
                    source.volume = SoundData.gameVolume / 100f;
                }
            }
            
            WDebug.Log($"Spawned object: {spawnedObject}, audio sources set: {audioSource.Length}");
            
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
            
            WDebug.Log($"Spawned object: {createdObjectDto.type}, id: {createdObjectDto.id} at position {position}");
        }
    }
}
using Script.Data.Sound;
using Script.GameScene.Exception;
using Script.Global;
using UnityEngine;

namespace Script.GameScene.Object
{
    public class ObjectSpawner : LocalSingletonObject<ObjectSpawner>
    {
        public void SpawnObject(CreatedObjectDto createdObjectDto)
        {
            WDebug.Log($"Spawning object: {createdObjectDto.type}, id: {createdObjectDto.id}");
            Vector3 position = new Vector3(
                createdObjectDto.position.x, 
                createdObjectDto.position.y, 
                createdObjectDto.position.z);
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{createdObjectDto.type}");
            
            if (prefab == null)
            {
                WDebug.LogWarning($"Prefab not found for type: {createdObjectDto.type}");
                return;
            }
            
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
            
            servedObject.id = createdObjectDto.id;
            try
            {
                ObjectContainer.Instance.RegisterObject(servedObject);
            } catch (DuplicatedException e)
            {
                WDebug.LogError($"Failed to register object: {e.Message}");
                Destroy(servedObject);
            }
        }
    }
}
using Script.GameScene.Exception;
using UnityEngine;

namespace Script.GameScene
{
    public class ObjectSpawner : MonoBehaviour
    {
        public static ObjectSpawner Instance { get; private set; }
        
        [SerializeField] private PlayerAnimationTrigger leftPlayerAnimationTrigger;
        [SerializeField] private PlayerAnimationTrigger rightPlayerAnimationTrigger;
        

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
                case "RightPlayer":
                    rightPlayerAnimationTrigger.UseMagic();   
                    break;
                case "LeftPlayer":
                    leftPlayerAnimationTrigger.UseMagic();
                    break;
                default:
                    Debug.LogWarning($"Unknown master: {createdObjectDto.master}");
                    break;
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
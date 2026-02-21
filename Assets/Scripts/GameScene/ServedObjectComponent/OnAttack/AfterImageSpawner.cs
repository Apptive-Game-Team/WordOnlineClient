using System.Collections;
using UnityEngine;

namespace GameScene.ServedObjectComponent.OnAttack
{
    public class AfterImageSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject AfterImagePrefab;
        private ServedObject servedObject;
        
        bool isSpawned = false;
        private void Start()
        {
            servedObject = transform.parent.GetComponentInChildren<ServedObject>();
            servedObject.OnAttack += SetSpawned;
            StartCoroutine(PlayAfterImage());
        }
        
        private void OnDestroy()
        {
            servedObject.OnAttack -= SetSpawned;
        }


        private void SetSpawned()
        {
            isSpawned = true;
        }
        
        private IEnumerator PlayAfterImage()
        {
            while (true)
            {
                if (isSpawned)
                {
                    Instantiate(AfterImagePrefab, transform.position, Quaternion.identity);
                    yield return new WaitForSeconds(0.05f);        
                }
            }
        }
    }
}

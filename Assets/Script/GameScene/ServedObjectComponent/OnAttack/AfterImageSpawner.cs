using System.Collections;
using Script.Data.Sound;
using UnityEngine;

namespace Script.GameScene.ServedObjectComponent.OnAttack
{
    public class AfterImageSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject AfterImagePrefab;
        
        bool isSpawned = false;
        private void Start()
        {
            ServedObject servedObject = transform.parent.GetComponentInChildren<ServedObject>();
            servedObject.OnAttack += SetSpawned;
            StartCoroutine(PlayAfterImage());
        }


        public void SetSpawned()
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

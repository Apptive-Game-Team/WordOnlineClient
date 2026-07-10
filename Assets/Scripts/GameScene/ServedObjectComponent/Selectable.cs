using GameScene.Card;
using Global;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameScene.ServedObjectComponent
{
    public class Selectable : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Camera worldCamera;

        void Awake()
        {
            if (!worldCamera) worldCamera = Camera.main;
        
            var sr  = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            GameObject colliderObject = sr != null ? sr.gameObject : gameObject;
            var box = colliderObject.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = colliderObject.AddComponent<BoxCollider>();
            }

            if (box == null)
            {
                WDebug.LogWarning($"{nameof(Selectable)} could not create a BoxCollider for {colliderObject.name}.");
                return;
            }

            if (sr && sr.sprite)
            {
                var b = sr.sprite.bounds;
                box.size = new Vector3(b.size.x, b.size.y, 0.1f);
                box.center = new Vector3(b.center.x, b.center.y, 0f);
                box.isTrigger = true;
            }
        
            if (worldCamera && !worldCamera.GetComponent<PhysicsRaycaster>())
                worldCamera.gameObject.AddComponent<PhysicsRaycaster>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Transform target = transform.parent != null ? transform.parent : transform;
            Vector3 pos = target.position;
        
            CardInputSender.Instance.SendInput(pos);
        }
    }
}

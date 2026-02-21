using Scripts.GameScene.Card;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scripts.GameScene.ServedObjectComponent
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Selectable : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private CardInputSender cardInputSender;
        [SerializeField] private Camera worldCamera;

        void Awake()
        {
            if (!worldCamera) worldCamera = Camera.main;
            if (!cardInputSender) cardInputSender = FindObjectOfType<CardInputSender>();
        
            var box = GetComponent<BoxCollider2D>();
            var sr  = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            if (sr && sr.sprite)
            {
                var b = sr.sprite.bounds;
                box.size   = b.size;
                box.offset = b.center;
                box.isTrigger = true;
            }
        
            if (worldCamera && !worldCamera.GetComponent<Physics2DRaycaster>())
                worldCamera.gameObject.AddComponent<Physics2DRaycaster>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!cardInputSender) return;
            var pos = transform.parent.position;
        
            cardInputSender.SendInput(pos);
        }
    }
}
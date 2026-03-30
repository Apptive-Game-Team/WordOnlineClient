using UnityEngine;
using UnityEngine.UI;

namespace GameScene.ServedObjectComponent
{
    public class ServedObjectHpBar : MonoBehaviour
    {
        [SerializeField] private ServedObject servedObject;
        private Slider slider;
    
        private void Awake()
        {
            slider = GetComponentInChildren<Slider>();
        }
    
        void Update()
        {
            if (servedObject == null)
            {
                servedObject = GetComponentInParent<ServedObject>();
                return;
            }
        
            slider.maxValue = servedObject.maxHp;
            slider.value = servedObject.hp;
        }
    }
}

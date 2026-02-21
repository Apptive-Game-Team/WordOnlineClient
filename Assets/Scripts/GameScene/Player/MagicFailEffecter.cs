using Scripts.GameScene.ServedObjectComponent;
using UnityEngine;

namespace Scripts.GameScene.Player
{
    public class MagicFailEffecter : MonoBehaviour
    {
        [SerializeField] private GameObject effect;
    
        private void Start()
        {
            effect.SetActive(false);
        }

        public void Trigger()
        {
            effect.SetActive(true);
            DOTweenAction.Pop(effect.transform);
        }
    }
}

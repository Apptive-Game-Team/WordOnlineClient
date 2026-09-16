using GameScene.ServedObjectComponent;
using Global;
using UnityEngine;

namespace GameScene.Player
{
    public class PlayerActionController : MonoBehaviour
    {
    
        [SerializeField] private ServedObject servedObject;
        [SerializeField] private MagicFailEffecter magicFailEffecter;
        private GameObject playerObject;

        private void Start()
        {
            if (servedObject == null)
            {
                servedObject = GetComponent<ServedObject>();
            }

            if (servedObject == null)
            {
                WDebug.LogWarning($"{nameof(PlayerActionController)} requires a {nameof(ServedObject)}.");
                return;
            }

            servedObject.OnOtherStatus += OnOtherStatus;
        }

        private void OnDestroy()
        {
            if (servedObject == null) return;

            servedObject.OnOtherStatus -= OnOtherStatus;
        }

        private void OnOtherStatus(string status)
        {
            if (status.Equals("Hindered"))
            {
                magicFailEffecter?.Trigger();
            }
        }
    }
}

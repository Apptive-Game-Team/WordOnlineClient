using UnityEngine;

namespace Scripts.TutorialScene
{
    public class ShotMocker : MonoBehaviour
    {
        void Update()
        {
            transform.Translate(Vector3.right * Time.deltaTime * 5f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TutorialSceneUIController.Instance.HPMocking(50);
            Destroy(this.gameObject);
        }
    }
}

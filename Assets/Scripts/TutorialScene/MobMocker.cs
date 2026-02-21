using UnityEngine;

namespace TutorialScene
{
    public class MobMocker : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Destroy(this.gameObject, 5);
        }
    }
}

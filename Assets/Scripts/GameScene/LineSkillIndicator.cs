using Global;
using UnityEngine;

namespace GameScene
{
    public class LineSkillIndicator : MonoBehaviour
    {
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void OnEnable()
        {
            if (SceneContext.Me.Equals("RightPlayer"))
            {
                transform.position = new Vector3(17, 5, 0);
            }
            else
            {
                transform.position = new Vector3(1, 5, 0);
            }   
        }

        void Update()
        {
            Vector3 mouseWorld = _camera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            Vector3 direction = mouseWorld - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}

using UnityEngine;

namespace GameScene
{
    public class CircleSkillIndicator : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float zPlane = 0f; // 마우스를 투영할 월드 z값

        void Awake()
        {
            if (!cam) cam = Camera.main; // MainCamera 태그 확인!
        }

        void Update()
        {
            if (!cam) return;

            var sp = Input.mousePosition;
            // 카메라에서 zPlane까지의 거리
            float dist = Mathf.Abs(zPlane - cam.transform.position.z);
            sp.z = dist;

            var world = cam.ScreenToWorldPoint(sp);
            world.z = zPlane; // 정확히 원하는 z로 고정
            transform.position = world;
        }
    }
}

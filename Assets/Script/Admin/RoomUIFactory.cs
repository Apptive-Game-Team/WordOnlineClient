using TMPro;
using UnityEngine;

namespace Script.Admin
{
    public class RoomUIFactory : MonoBehaviour
    {
        [SerializeField] private AdminViewModel adminViewModel;
        [SerializeField] private GameObject roomUIPrefab;
        [SerializeField] private Transform contentTransform;
        
        private void Start()
        {
            adminViewModel.FetchRoomList(OnRoomListFetched);
        }
        
        private void OnRoomListFetched(Dto.RoomList roomList)
        {
            foreach (var roomInfo in roomList.rooms)
            {
                var roomUIGameObject = Instantiate(roomUIPrefab, contentTransform);
                var roomUI = roomUIGameObject.GetComponent<RoomUI>();
                roomUI.SetRoomInfo(roomInfo);
            }
        }
        
        public void ReloadRoomList()
        {
            foreach (Transform child in contentTransform)
            {
                Destroy(child.gameObject);
            }
            adminViewModel.FetchRoomList(OnRoomListFetched);
        }
    }
}
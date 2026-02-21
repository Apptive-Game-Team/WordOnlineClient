using System;
using Scripts.Admin.Client;
using Scripts.Admin.Dto;
using UnityEngine;

namespace Scripts.Admin
{
    public class AdminViewModel : MonoBehaviour
    {
        private readonly RoomApiClient roomApiClient = new RoomApiClient();

        public void FetchRoomList(Action<RoomList> callback)
        {
            StartCoroutine(roomApiClient.GetRoomList(callback));
        }
    }
}
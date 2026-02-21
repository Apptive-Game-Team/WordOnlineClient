using System;
using Admin.Client;
using Admin.Dto;
using UnityEngine;

namespace Admin
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
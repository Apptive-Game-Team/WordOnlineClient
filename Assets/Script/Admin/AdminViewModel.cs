using System;
using Script.Admin.Client;
using Script.Admin.Dto;
using UnityEngine;

namespace Script.Admin
{
    public class AdminViewModel : MonoBehaviour
    {
        private RoomApiClient roomApiClient = new RoomApiClient();

        public void FetchRoomList(Action<RoomList> callback)
        {
            StartCoroutine(roomApiClient.GetRoomList(callback));
        }
    }
}
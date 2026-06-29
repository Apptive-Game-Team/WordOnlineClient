using System;
using Admin.Client;
using Admin.Dto;
using Data;
using UnityEngine;

namespace Admin
{
    public class AdminViewModel : MonoBehaviour
    {
        private readonly RoomApiClient roomApiClient = new RoomApiClient();
        private readonly DebugApiClient debugApiClient = new DebugApiClient();
        private readonly AdminMatchApiClient adminMatchApiClient = new AdminMatchApiClient();

        public void FetchRoomList(Action<RoomList> callback)
        {
            StartCoroutine(roomApiClient.GetRoomList(callback));
        }

        public void SummonMagic(DebugSummonMagicRequestDto dto, Action<DebugActionResponseDto> callback)
        {
            StartCoroutine(debugApiClient.SummonMagic(dto, callback));
        }

        public void SpawnPrefab(DebugSpawnPrefabRequestDto dto, Action<DebugActionResponseDto> callback)
        {
            StartCoroutine(debugApiClient.SpawnPrefab(dto, callback));
        }

        public void MatchBots(BotMatchRequestDto dto, Action<MatchedInfoDto> callback)
        {
            StartCoroutine(adminMatchApiClient.MatchBots(dto, callback));
        }

        public void FetchMagics(Action<DebugMagicDto[]> callback)
        {
            StartCoroutine(debugApiClient.GetMagics(callback));
        }

        public void FetchPrefabs(Action<DebugPrefabDto[]> callback)
        {
            StartCoroutine(debugApiClient.GetPrefabs(callback));
        }
    }
}

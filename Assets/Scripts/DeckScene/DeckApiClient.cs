using System;
using System.Collections;
using System.Text;
using Data;
using Data.Deck;
using Global;
using Global.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace DeckScene
{
    public class DeckApiClient
    {
        public IEnumerator GetOwnedCards(Action<CardDto[]> callback)
        {
            string url = $"{ServerList.MatchingServer.url}/api/users/mine/cardLists";
            using var request = UnityWebRequest.Get(url);

            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError(
                    $"보유 카드 풀 로드 실패: {request.responseCode} / {request.error}\n{request.downloadHandler.text}"
                );
                callback?.Invoke(null);
                yield break;
            }

            WDebug.Log($"보유 카드 리스트: {request.downloadHandler.text}");
            CardPoolDto poolDto = JsonUtility.FromJson<CardPoolDto>(request.downloadHandler.text);
            callback?.Invoke(poolDto?.cards ?? Array.Empty<CardDto>());
        }

        public IEnumerator GetDecks(Action<DeckResponseDto[]> callback)
        {
            string url = $"{ServerList.MatchingServer.url}/api/users/mine/decks";
            using var request = UnityWebRequest.Get(url);

            Server.SetAcceptLanguage(request);
            Server.SetAuthorization(request);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                WDebug.LogError(
                    $"유저 덱 리스트 로드 실패: {request.responseCode} / {request.error}\n{request.downloadHandler.text}"
                );
                callback?.Invoke(null);
                yield break;
            }

            WDebug.Log($"유저 덱 리스트: {request.downloadHandler.text}");
            callback?.Invoke(JsonHelper.FromJson<DeckResponseDto>(request.downloadHandler.text) ?? Array.Empty<DeckResponseDto>());
        }

        public IEnumerator CreateDeck(DeckRequestDto dto, Action<bool> callback)
        {
            string url = $"{ServerList.MatchingServer.url}/api/users/mine/decks";
            yield return SendDeckRequest(url, "POST", dto, "POST 덱 페이로드", "덱 생성 실패", callback);
        }

        public IEnumerator UpdateDeck(long deckId, DeckRequestDto dto, Action<bool> callback)
        {
            string url = $"{ServerList.MatchingServer.url}/api/users/mine/decks/{deckId}";
            yield return SendDeckRequest(url, "PUT", dto, "PUT 덱 페이로드", "덱 수정 실패", callback);
        }

        private IEnumerator SendDeckRequest(
            string url,
            string method,
            DeckRequestDto dto,
            string payloadLogPrefix,
            string failureLogPrefix,
            Action<bool> callback)
        {
            string json = JsonUtility.ToJson(dto);
            WDebug.Log($"{payloadLogPrefix}: {json}");

            using var request = new UnityWebRequest(url, method)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            Server.SetAuthorization(request);
            Server.SetAcceptLanguage(request);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            bool isSuccess = request.result == UnityWebRequest.Result.Success;
            if (!isSuccess)
            {
                WDebug.LogError($"{failureLogPrefix}: {request.responseCode} / {request.error}\n{request.downloadHandler.text}");
            }
            else
            {
                WDebug.Log($"{method} 덱 성공: {request.downloadHandler.text}");
            }

            callback?.Invoke(isSuccess);
        }
    }
}

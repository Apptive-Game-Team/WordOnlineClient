using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Data;
using Data.Profile;
using Global;
using UnityEngine;
using UnityEngine.Networking;

namespace ProfileScene
{
    public class GameHistoryItemFactory : MonoBehaviour
    {
        private static readonly Dictionary<long, string> opponentUsernameCache = new Dictionary<long, string>();

        [SerializeField] private GameHistoryItem itemPrefab;
        [SerializeField] private Transform itemRoot;
        [SerializeField] private bool hideTemplateOnAwake = true;

        private void Awake()
        {
            EnsureItemRoot();
            EnsureItemPrefab();

            if (hideTemplateOnAwake && itemPrefab != null)
            {
                itemPrefab.gameObject.SetActive(false);
            }
        }

        public void SetItemRoot(Transform root)
        {
            itemRoot = root;
            EnsureItemPrefab();
        }

        public void ClearItems()
        {
            EnsureItemRoot();
            if (itemRoot == null)
            {
                return;
            }

            for (int i = itemRoot.childCount - 1; i > 0; i--)
            {
                Transform child = itemRoot.GetChild(i);
                if (itemPrefab != null && child == itemPrefab.transform)
                {
                    continue;
                }

                Destroy(child.gameObject);
            }
        }

        public IEnumerator CreateRange(UserGameHistoryDto[] games)
        {
            if (games == null)
            {
                yield break;
            }

            foreach (UserGameHistoryDto game in games)
            {
                yield return Create(game, _ => { });
            }
        }

        public IEnumerator Create(UserGameHistoryDto gameHistory, Action<GameHistoryItem> callback)
        {
            EnsureItemRoot();
            EnsureItemPrefab();

            if (itemPrefab == null)
            {
                callback?.Invoke(null);
                yield break;
            }

            Transform parent = itemRoot == null ? transform : itemRoot;
            GameHistoryItem item = Instantiate(itemPrefab, parent);
            item.gameObject.SetActive(true);
            yield return Render(item, gameHistory);
            callback?.Invoke(item);
        }

        public IEnumerator BuildHistoryText(UserGameHistoryDto[] games, Action<string> callback)
        {
            if (games == null || games.Length == 0)
            {
                callback?.Invoke("No games yet.");
                yield break;
            }

            StringBuilder builder = new StringBuilder();
            int count = Math.Min(games.Length, 20);

            for (int i = 0; i < count; i++)
            {
                UserGameHistoryDto game = games[i];
                if (game == null)
                {
                    continue;
                }

                string opponentUsername = null;

                yield return GetOpponentUsername(game.OpponentId, username => opponentUsername = username);

                builder.Append(game.IsWin ? "Win" : "Loss")
                    .Append(" vs ")
                    .Append(opponentUsername)
                    .AppendLine();
            }

            callback?.Invoke(builder.Length == 0 ? "No games yet." : builder.ToString());
        }

        public IEnumerator Render(GameHistoryItem item, UserGameHistoryDto gameHistory)
        {
            if (item == null)
            {
                yield break;
            }

            string opponentUsername = null;
            yield return GetOpponentUsername(gameHistory?.OpponentId ?? 0, username => opponentUsername = username);
            item.Render(gameHistory, opponentUsername);
        }

        private static IEnumerator GetOpponentUsername(long opponentId, Action<string> callback)
        {
            if (opponentId <= 0)
            {
                callback?.Invoke("Unknown");
                yield break;
            }

            if (opponentUsernameCache.TryGetValue(opponentId, out string cachedUsername))
            {
                callback?.Invoke(cachedUsername);
                yield break;
            }

            string fallbackUsername = $"#{opponentId}";
            string url = $"{ServerList.AccountServer.url}/api/members/{opponentId}";
            using UnityWebRequest webRequest = UnityWebRequest.Get(url);

            Server.SetAcceptLanguage(webRequest);
            Server.SetAuthorization(webRequest);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                AccountUser accountUser = JsonUtility.FromJson<AccountUser>(webRequest.downloadHandler.text);
                string username = string.IsNullOrEmpty(accountUser?.DisplayName) ? fallbackUsername : accountUser.DisplayName;
                opponentUsernameCache[opponentId] = username;
                callback?.Invoke(username);
                yield break;
            }

            WDebug.LogError($"Failed to get opponent username: {opponentId}, {webRequest.responseCode} / {webRequest.error}");
            opponentUsernameCache[opponentId] = fallbackUsername;
            callback?.Invoke(fallbackUsername);
        }

        private void EnsureItemRoot()
        {
            if (itemRoot == null)
            {
                itemRoot = transform;
            }
        }

        private void EnsureItemPrefab()
        {
            if (itemPrefab != null || itemRoot == null)
            {
                return;
            }

            itemPrefab = itemRoot.GetComponentInChildren<GameHistoryItem>(true);
        }
    }
}

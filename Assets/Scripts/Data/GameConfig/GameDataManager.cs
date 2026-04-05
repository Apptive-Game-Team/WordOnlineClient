using System;
using System.Collections;
using Global;
using UnityEngine;
using UnityEngine.Networking;

namespace Data.GameConfig
{
    /// <summary>
    /// Downloads and caches versioned game configuration (magic recipes, parameters,
    /// elemental chart, card definitions) from the server.
    ///
    /// Usage:
    ///   yield return GameDataManager.Instance.Initialize();
    ///   var config = GameDataManager.Config;
    ///
    /// Call Initialize() once at app start before any scene that needs game data.
    /// The result is cached in PlayerPrefs so subsequent launches only do a lightweight
    /// version check.
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        private const string PrefKeyVersion = "GameData.Version";
        private const string PrefKeyConfig  = "GameData.Config";

        public static GameDataManager Instance { get; private set; }

        /// <summary>The live game configuration. Null until Initialize() completes.</summary>
        public static GameConfigData Config { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Coroutine: checks server version, loads from PlayerPrefs cache if up-to-date,
        /// or downloads fresh config if stale/missing. Falls back to cached data on network failure.
        /// </summary>
        public IEnumerator Initialize(Action<bool> onComplete = null)
        {
            string serverVersion = null;
            yield return FetchVersion(v => serverVersion = v);

            string cachedVersion = PlayerPrefs.GetString(PrefKeyVersion, "");

            if (serverVersion != null
                && cachedVersion == serverVersion
                && PlayerPrefs.HasKey(PrefKeyConfig))
            {
                LoadFromPrefs();
                WDebug.Log($"[GameDataManager] Loaded from cache (version={cachedVersion})");
                onComplete?.Invoke(true);
                yield break;
            }

            if (serverVersion == null && PlayerPrefs.HasKey(PrefKeyConfig))
            {
                // Network unavailable but we have a cached copy — use it
                LoadFromPrefs();
                WDebug.LogWarning("[GameDataManager] Network unavailable, using stale cache");
                onComplete?.Invoke(true);
                yield break;
            }

            if (serverVersion == null)
            {
                WDebug.LogError("[GameDataManager] No network and no cache. Cannot initialize.");
                onComplete?.Invoke(false);
                yield break;
            }

            // Cache is stale or missing — fetch full config
            GameConfigData freshConfig = null;
            yield return FetchConfig(c => freshConfig = c);

            if (freshConfig == null)
            {
                WDebug.LogError("[GameDataManager] Failed to fetch game config.");
                onComplete?.Invoke(false);
                yield break;
            }

            Config = freshConfig;
            SaveToPrefs(serverVersion, freshConfig);
            WDebug.Log($"[GameDataManager] Config refreshed (version={serverVersion})");
            onComplete?.Invoke(true);
        }

        private void LoadFromPrefs()
        {
            string json = PlayerPrefs.GetString(PrefKeyConfig);
            Config = JsonUtility.FromJson<GameConfigData>(json);
        }

        private static void SaveToPrefs(string version, GameConfigData config)
        {
            string json = JsonUtility.ToJson(config);
            PlayerPrefs.SetString(PrefKeyConfig, json);
            PlayerPrefs.SetString(PrefKeyVersion, version);
            PlayerPrefs.Save();
        }

        private IEnumerator FetchVersion(Action<string> callback)
        {
            string url = $"{ServerList.MatchingServer.url}/api/data/version";
            using UnityWebRequest req = UnityWebRequest.Get(url);
            Server.SetAcceptLanguage(req);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GameDataVersionResponse>(req.downloadHandler.text);
                callback(response?.version);
            }
            else
            {
                WDebug.LogWarning($"[GameDataManager] Version check failed: {req.error}");
                callback(null);
            }
        }

        private IEnumerator FetchConfig(Action<GameConfigData> callback)
        {
            string url = $"{ServerList.MatchingServer.url}/api/data/config";
            using UnityWebRequest req = UnityWebRequest.Get(url);
            Server.SetAcceptLanguage(req);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var config = JsonUtility.FromJson<GameConfigData>(req.downloadHandler.text);
                callback(config);
            }
            else
            {
                WDebug.LogError($"[GameDataManager] Config fetch failed: {req.error}");
                callback(null);
            }
        }
    }
}

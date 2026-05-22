using System;
using System.Collections;
using UnityEngine;

namespace Data.Versioning
{
    public abstract class VersionedDataSource<TClient, TResponse> : MonoBehaviour
        where TClient : VersionedApiClient<TResponse>, new()
        where TResponse : class, IVersionedResponse
    {
        protected abstract string PlayerPrefsKey { get; }

        protected TClient Client { get; private set; }
        protected string Version { get; set; }
        protected string SourceUrl { get; private set; }

        protected virtual void Awake()
        {
            Client = new TClient();
            LoadFromPlayerPrefs();
        }

        protected IEnumerator UpdateData()
        {
            // A source URL change means the old version token is no longer valid for delta fetches.
            if (!string.Equals(SourceUrl, Client.SourceUrl, StringComparison.Ordinal))
            {
                Version = null;
            }

            yield return Client.Get(response =>
            {
                if (response != null)
                {
                    if (response.RequiresRefresh)
                    {
                        ProcessResponse(response);
                    }

                    ApplyFetchedMetadata(response);
                    SaveToPlayerPrefs();
                }
            }, Version);
        }

        protected void LoadFromPlayerPrefs()
        {
            InitializeData();
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                return;
            }

            var json = PlayerPrefs.GetString(PlayerPrefsKey);
            var saved = JsonUtility.FromJson<TResponse>(json);
            if (saved != null)
            {
                ProcessResponse(saved);
                ApplyCachedMetadata(saved);
            }
        }

        protected virtual void SaveToPlayerPrefs()
        {
            var saved = BuildSaveResponse();
            if (saved == null)
            {
                return;
            }

            // Derived data sources provide the domain payload; the base layer stamps shared metadata.
            saved.RequiresRefresh = true;
            saved.Version = Version;
            saved.SourceUrl = SourceUrl ?? Client.SourceUrl;

            var json = JsonUtility.ToJson(saved);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        private void ApplyCachedMetadata(TResponse response)
        {
            if (!string.IsNullOrEmpty(response.Version))
            {
                Version = response.Version;
            }

            SourceUrl = response.SourceUrl;
        }

        private void ApplyFetchedMetadata(TResponse response)
        {
            if (!string.IsNullOrEmpty(response.Version))
            {
                Version = response.Version;
            }

            SourceUrl = !string.IsNullOrEmpty(response.SourceUrl)
                ? response.SourceUrl
                : Client.SourceUrl;
        }

        protected abstract void InitializeData();
        protected abstract void ProcessResponse(TResponse response);
        protected abstract TResponse BuildSaveResponse();
    }
}

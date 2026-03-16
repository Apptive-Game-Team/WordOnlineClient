using System.Collections;
using UnityEngine;

namespace Data.Versioning
{
    public abstract class VersionedDataSource<TClient, TResponse> : MonoBehaviour
        where TClient : VersionedApiClient<TResponse>, new()
    {
        protected abstract string PlayerPrefsKey { get; }

        protected TClient Client { get; private set; }
        protected string Version { get; set; }

        protected virtual void Awake()
        {
            Client = new TClient();
            LoadFromPlayerPrefs();
        }

        protected IEnumerator UpdateData()
        {
            yield return Client.Get(response =>
            {
                if (response != null)
                {
                    ProcessResponse(response);
                    SaveToPlayerPrefs();
                }
            }, Version);
        }

        protected void LoadFromPlayerPrefs()
        {
            InitializeData();
            if (PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                var json = PlayerPrefs.GetString(PlayerPrefsKey);
                var saved = JsonUtility.FromJson<TResponse>(json);
                ProcessResponse(saved);
            }
        }

        protected abstract void InitializeData();
        protected abstract void ProcessResponse(TResponse response);
        protected abstract void SaveToPlayerPrefs();
    }
}

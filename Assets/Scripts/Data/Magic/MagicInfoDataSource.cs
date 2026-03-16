using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Magic
{
    public class MagicInfoDataSource : MonoBehaviour
    {
        private const string PlayerPrefsKey = "MagicInfoData";
        private MagicInfoApiClient client;
        private List<MagicInfoDto> magics;
        private long? version;

        private void Awake()
        {
            client = new MagicInfoApiClient();
            LoadFromPlayerPrefs();
        }

        private IEnumerator UpdateMagicInfo()
        {
            yield return client.GetMagicInfo(response =>
            {
                if (response != null)
                {
                    ProcessResponse(response);
                    SaveToPlayerPrefs();
                }
            }, version?.ToString());
        }

        private void ProcessResponse(MagicInfoResponse response)
        {
            version = response.version;
            magics = response.magics;
        }

        private void LoadFromPlayerPrefs()
        {
            if (PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                var json = PlayerPrefs.GetString(PlayerPrefsKey);
                var saved = JsonUtility.FromJson<MagicInfoResponse>(json);
                version = saved.version;
                magics = saved.magics ?? new List<MagicInfoDto>();
            }
            else
            {
                magics = new List<MagicInfoDto>();
            }
        }

        private void SaveToPlayerPrefs()
        {
            var cached = new MagicInfoResponse { version = version ?? 0, magics = magics };
            var json = JsonUtility.ToJson(cached);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        public List<MagicInfoDto> GetMagics()
        {
            return magics;
        }

        public void GetMagics(System.Action<List<MagicInfoDto>> callback)
        {
            StartCoroutine(GetMagicsRoutine(callback));
        }

        private IEnumerator GetMagicsRoutine(System.Action<List<MagicInfoDto>> callback)
        {
            yield return UpdateMagicInfo();
            callback?.Invoke(magics);
        }
    }
}

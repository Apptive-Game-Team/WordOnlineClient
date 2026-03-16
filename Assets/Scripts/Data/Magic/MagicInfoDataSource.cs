using System;
using System.Collections;
using System.Collections.Generic;
using Data.Versioning;
using UnityEngine;

namespace Data.Magic
{
    public class MagicInfoDataSource : VersionedDataSource<MagicInfoApiClient, MagicInfoResponse>
    {
        protected override string PlayerPrefsKey => "MagicInfoData";

        private List<MagicInfoDto> magics;

        protected override void InitializeData()
        {
            magics = new List<MagicInfoDto>();
        }

        protected override void ProcessResponse(MagicInfoResponse response)
        {
            Version = response.version;
            if (response.magics != null)
            {
                magics = response.magics;
            }
        }

        protected override void SaveToPlayerPrefs()
        {
            var json = JsonUtility.ToJson(new MagicInfoResponse { version = Version, magics = magics });
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        public List<MagicInfoDto> GetMagics()
        {
            return magics;
        }

        public void GetMagics(Action<List<MagicInfoDto>> callback)
        {
            StartCoroutine(GetMagicsRoutine(callback));
        }

        private IEnumerator GetMagicsRoutine(Action<List<MagicInfoDto>> callback)
        {
            yield return UpdateData();
            callback?.Invoke(magics);
        }
    }
}

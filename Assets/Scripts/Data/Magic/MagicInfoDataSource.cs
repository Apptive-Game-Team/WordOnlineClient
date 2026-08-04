using System;
using System.Collections;
using System.Collections.Generic;
using Data.Versioning;

namespace Data.Magic
{
    public class MagicInfoDataSource : VersionedDataSource<MagicInfoDataSource, MagicInfoApiClient, MagicInfoResponse>
    {
        public const string PlayerPrefsKeyName = "MagicInfoData";

        private const string RuntimeObjectName = nameof(MagicInfoDataSource);

        protected override string PlayerPrefsKey => PlayerPrefsKeyName;

        private List<MagicInfoDto> magics;

        public new static MagicInfoDataSource Instance => GetOrCreateInstance(RuntimeObjectName);

        public static IReadOnlyList<MagicInfoDto> GetCachedMagics() => Instance.magics;

        public static string GetCachedVersion() => Instance.CachedVersion;

        protected override void InitializeData()
        {
            magics = new List<MagicInfoDto>();
        }

        protected override void ProcessResponse(MagicInfoResponse response)
        {
            // Server payloads replace the cached set so removed magics do not linger locally.
            magics = response.magics != null
                ? new List<MagicInfoDto>(response.magics)
                : new List<MagicInfoDto>();
        }

        protected override MagicInfoResponse BuildSaveResponse()
        {
            return new MagicInfoResponse
            {
                magics = magics ?? new List<MagicInfoDto>()
            };
        }

        public void GetMagics(Action<List<MagicInfoDto>> callback)
        {
            callback?.Invoke(magics);
        }

        public void RefreshMagics(Action<List<MagicInfoDto>> callback = null)
        {
            StartCoroutine(RefreshMagicsRoutine(callback));
        }

        private IEnumerator RefreshMagicsRoutine(Action<List<MagicInfoDto>> callback)
        {
            yield return UpdateData();
            callback?.Invoke(magics);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Data.Versioning;
using UnityEngine;

namespace Data.Magic
{
    public class MagicInfoDataSource : VersionedDataSource<MagicInfoApiClient, MagicInfoResponse>
    {
        public const string PlayerPrefsKeyName = "MagicInfoData";

        private const string RuntimeObjectName = nameof(MagicInfoDataSource);

        private static MagicInfoDataSource instance;

        protected override string PlayerPrefsKey => PlayerPrefsKeyName;

        private List<MagicInfoDto> magics;

        public static MagicInfoDataSource Instance
        {
            get
            {
                EnsureInstance();
                return instance;
            }
        }

        public static IReadOnlyList<MagicInfoDto> GetCachedMagics()
        {
            return Instance.magics;
        }

        protected override void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void InitializeData()
        {
            magics = new List<MagicInfoDto>();
        }

        protected override void ProcessResponse(MagicInfoResponse response)
        {
            if (response.magics != null)
            {
                magics = new List<MagicInfoDto>(response.magics);

                return;
            }

            if (magics == null || magics.Count == 0)
            {
                Version = null;
            }
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

        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            var existing = FindObjectOfType<MagicInfoDataSource>();
            if (existing != null)
            {
                instance = existing;
                return;
            }

            var host = new GameObject(RuntimeObjectName);
            instance = host.AddComponent<MagicInfoDataSource>();
        }
    }
}

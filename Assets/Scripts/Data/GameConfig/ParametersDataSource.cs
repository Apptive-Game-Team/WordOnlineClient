using System;
using System.Collections;
using System.Collections.Generic;
using Data.Versioning;

namespace Data.GameConfig
{
    public class ParametersDataSource : VersionedDataSource<ParametersDataSource, ParametersApiClient, ParametersResponse>
    {
        public const string PlayerPrefsKeyName = "GameParametersData";

        private const string RuntimeObjectName = nameof(ParametersDataSource);

        protected override string PlayerPrefsKey => PlayerPrefsKeyName;

        private List<GameParameterData> parameters;

        public new static ParametersDataSource Instance => GetOrCreateInstance(RuntimeObjectName);

        public static IReadOnlyList<GameParameterData> GetCachedParameters() => Instance.parameters;

        protected override void InitializeData()
        {
            parameters = new List<GameParameterData>();
        }

        protected override void ProcessResponse(ParametersResponse response)
        {
            parameters = response.parameters != null
                ? new List<GameParameterData>(response.parameters)
                : new List<GameParameterData>();
        }

        protected override ParametersResponse BuildSaveResponse()
        {
            return new ParametersResponse
            {
                parameters = parameters ?? new List<GameParameterData>()
            };
        }

        public void GetParameters(Action<List<GameParameterData>> callback)
        {
            callback?.Invoke(parameters);
        }

        public void RefreshParameters(Action<List<GameParameterData>> callback = null)
        {
            StartCoroutine(RefreshParametersRoutine(callback));
        }

        private IEnumerator RefreshParametersRoutine(Action<List<GameParameterData>> callback)
        {
            yield return UpdateData();
            callback?.Invoke(parameters);
        }
    }
}

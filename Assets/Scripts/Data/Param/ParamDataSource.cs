using System;
using System.Collections;
using System.Collections.Generic;
using Data.Versioning;
using UnityEngine;

namespace Data.Param
{
    public class ParamDataSource : VersionedDataSource<ParamApiClient, ParametersResponse>
    {
        protected override string PlayerPrefsKey => "GameParameters";

        private Dictionary<string, Dictionary<string, double>> paramMap;

        protected override void InitializeData()
        {
            paramMap = new Dictionary<string, Dictionary<string, double>>();
        }

        protected override void ProcessResponse(ParametersResponse response)
        {
            Version = response.version;
            foreach (var param in response.parameters)
            {
                if (!paramMap.ContainsKey(param.gameObjectName))
                {
                    paramMap[param.gameObjectName] = new Dictionary<string, double>();
                }
                paramMap[param.gameObjectName][param.paramName] = param.value;
            }
        }

        protected override void SaveToPlayerPrefs()
        {
            var parameters = new List<Parameter>();
            foreach (var gameObjectEntry in paramMap)
            {
                foreach (var paramEntry in gameObjectEntry.Value)
                {
                    parameters.Add(new Parameter
                    {
                        gameObjectName = gameObjectEntry.Key,
                        paramName = paramEntry.Key,
                        value = paramEntry.Value
                    });
                }
            }

            var json = JsonUtility.ToJson(new ParametersResponse { version = Version, parameters = parameters });
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        public Dictionary<string, Dictionary<string, double>> GetParamMap()
        {
            return paramMap;
        }

        public void GetParamMap(Action<Dictionary<string, Dictionary<string, double>>> callback)
        {
            StartCoroutine(GetParamMapRoutine(callback));
        }

        private IEnumerator GetParamMapRoutine(Action<Dictionary<string, Dictionary<string, double>>> callback)
        {
            yield return UpdateData();
            callback?.Invoke(paramMap);
        }
    }
}

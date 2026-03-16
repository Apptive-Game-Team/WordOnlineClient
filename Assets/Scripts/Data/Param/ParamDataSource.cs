using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Param
{
    public class ParamDataSource : MonoBehaviour
    {
        private const string PlayerPrefsKey = "GameParameters";
        private ParamApiClient client;
        private Dictionary<string, Dictionary<string, double>> paramMap;
        private string version;

        private void Awake()
        {
            client = new ParamApiClient();
            LoadFromPlayerPrefs();
        }

        private IEnumerator UpdateParameters()
        {
            yield return client.GetParameters(response =>
            {
                if (response != null)
                {
                    ProcessResponse(response);
                    SaveToPlayerPrefs();
                }
            }, version);
        }

        private void ProcessResponse(ParametersResponse response)
        {
            version = response.version;
            foreach (var param in response.parameters)
            {
                if (!paramMap.ContainsKey(param.gameObjectName))
                {
                    paramMap[param.gameObjectName] = new Dictionary<string, double>();
                }
                paramMap[param.gameObjectName][param.paramName] = param.value;
            }
        }

        private void LoadFromPlayerPrefs()
        {
            if (PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                var json = PlayerPrefs.GetString(PlayerPrefsKey);
                var savedParams = JsonUtility.FromJson<ParametersResponse>(json);
                paramMap = new Dictionary<string, Dictionary<string, double>>();
                version = savedParams.version;
                ProcessResponse(savedParams);
            }
            else
            {
                paramMap = new Dictionary<string, Dictionary<string, double>>();
            }
        }

        private void SaveToPlayerPrefs()
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

            var response = new ParametersResponse { version = this.version, parameters = parameters };
            var json = JsonUtility.ToJson(response);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        public Dictionary<string, Dictionary<string, double>> GetParamMap()
        {
            return paramMap;
        }

        public void GetParamMap(System.Action<Dictionary<string, Dictionary<string, double>>> callback)
        {
            StartCoroutine(GetParamMapRoutine(callback));
        }

        private IEnumerator GetParamMapRoutine(System.Action<Dictionary<string, Dictionary<string, double>>> callback)
        {
            yield return UpdateParameters();
            callback?.Invoke(paramMap);
        }
    }
}

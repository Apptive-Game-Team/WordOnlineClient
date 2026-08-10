using System;
using System.Collections.Generic;
using Data.Versioning;
using Newtonsoft.Json;

namespace Data.GameConfig
{
    [Serializable]
    public class ParametersResponse : IVersionedResponse
    {
        public bool requiresRefresh;
        public string version;
        public string source_url;
        public List<GameParameterData> parameters;

        [JsonIgnore]
        public bool RequiresRefresh
        {
            get => requiresRefresh;
            set => requiresRefresh = value;
        }

        [JsonIgnore]
        public string Version
        {
            get => version;
            set => version = value;
        }

        [JsonIgnore]
        public string SourceUrl
        {
            get => source_url;
            set => source_url = value;
        }
    }
}

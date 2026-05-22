using System;
using System.Collections.Generic;
using Data.Versioning;

namespace Data.GameConfig
{
    [Serializable]
    public class ParametersResponse : IVersionedResponse
    {
        public bool requiresRefresh;
        public string version;
        public string source_url;
        public List<GameParameterData> parameters;

        public bool RequiresRefresh
        {
            get => requiresRefresh;
            set => requiresRefresh = value;
        }

        public string Version
        {
            get => version;
            set => version = value;
        }

        public string SourceUrl
        {
            get => source_url;
            set => source_url = value;
        }
    }
}

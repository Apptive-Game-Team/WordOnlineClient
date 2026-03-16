using System.Collections.Generic;

namespace Data.Param
{
    [System.Serializable]
    public class ParametersResponse
    {
        public List<Parameter> parameters;
        public string version;
    }
}

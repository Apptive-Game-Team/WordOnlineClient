using Data.Versioning;

namespace Data.Param
{
    public class ParamApiClient : VersionedApiClient<ParametersResponse>
    {
        protected override string Endpoint =>
            $"{ServerList.MatchingServer.url}/api/data/parameters";
    }
}

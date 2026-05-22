using Global;
using Data.Versioning;

namespace Data.GameConfig
{
    public class ParametersApiClient : VersionedApiClient<ParametersResponse>
    {
        protected override string Endpoint => $"{ServerList.MatchingServer.url}/api/data/parameters";

        protected override void OnSuccessRawJson(string json)
        {
            WDebug.Log($"[ParametersApiClient] Response json: {json}");
        }
    }
}

using Global;
using Data.Versioning;

namespace Data.Magic
{
    public class MagicInfoApiClient : VersionedApiClient<MagicInfoResponse>
    {
        protected override string Endpoint => $"{ServerList.MatchingServer.url}/api/data/magics";

        protected override void OnSuccessRawJson(string json)
        {
            WDebug.Log($"[MagicInfoApiClient] Response json: {json}");
        }
    }
}

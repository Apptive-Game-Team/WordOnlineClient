using Data.Versioning;

namespace Data.Magic
{
    public class MagicInfoApiClient : VersionedApiClient<MagicInfoResponse>
    {
        protected override string Endpoint =>
            $"{ServerList.MatchingServer.url}/api/data/magics";
    }
}

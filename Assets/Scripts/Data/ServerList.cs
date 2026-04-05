using System;

namespace Data
{
    public static class ServerList
    {
        // public static readonly List<Server> Servers = new()
        // {
        //     new Server("춘천", "www.monolong.shop", 7777, true),
        //     new Server("춘천2", "www.monolong.shop", 6210, true),
        //     new Server("로컬", "localhost", 7777)
        // };

        private const string DevDeployType = "DEV";

        // When the DEPLOY_TYPE environment variable is set to "DEV", the matching server
        // uses the dev host (dev.lobby.ac.yunseong.dev); otherwise the production host is used.
        private static readonly string MatchingServerHost =
            Environment.GetEnvironmentVariable("DEPLOY_TYPE") == DevDeployType
                ? "dev.lobby.ac.yunseong.dev"
                : "lobby.ac.yunseong.dev";

        public static readonly Server MatchingServer = new Server("춘천", MatchingServerHost, 443, true);
        // public static readonly Server MatchingServer = new Server("춘천", "localhost", 6209, false);
        
        public static readonly Server AccountServer = new Server("춘천", "account.ac.yunseong.dev", 443, true);
    }
}
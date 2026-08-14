namespace Data
{
    public static class ServerList
    {
        /// <summary>
        /// Matching/lobby server currently in use. Admins can switch it at runtime from the lobby
        /// dropdown; see <see cref="MatchingServerCatalog"/> for the candidates and the default.
        /// </summary>
        public static Server MatchingServer => MatchingServerCatalog.Current.Server;

        public static readonly Server AccountServer = new Server("춘천", "account.ac.yunseong.dev", 443, true);
    }
}

namespace Scripts.Data
{
    public static class ServerList
    {
        // public static readonly List<Server> Servers = new()
        // {
        //     new Server("춘천", "www.monolong.shop", 7777, true),
        //     new Server("춘천2", "www.monolong.shop", 6210, true),
        //     new Server("로컬", "localhost", 7777)
        // };

        public static readonly Server MatchingServer = new Server("춘천", "www.monolong.shop", 6210, true);
        // public static readonly Server MatchingServer = new Server("춘천", "localhost", 6209, false);
        
        public static readonly Server AccountServer = new Server("춘천", "www.monolong.shop", 6203, true);
    }
}
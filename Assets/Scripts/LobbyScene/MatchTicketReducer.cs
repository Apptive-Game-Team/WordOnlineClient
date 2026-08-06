namespace LobbyScene
{
    public sealed class MatchTicketReducer
    {
        public MatchTicket Current { get; private set; }

        public bool Apply(MatchTicket incoming)
        {
            if (incoming == null || string.IsNullOrEmpty(incoming.ticketId)) return false;
            if (Current != null && incoming.ticketId == Current.ticketId && incoming.version <= Current.version)
                return false;

            Current = incoming;
            return true;
        }

        public void Clear()
        {
            Current = null;
        }
    }
}

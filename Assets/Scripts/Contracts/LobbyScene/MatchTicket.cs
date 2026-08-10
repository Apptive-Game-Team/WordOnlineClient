using System;
using Data;

namespace LobbyScene
{
    public enum MatchTicketState
    {
        Unknown,
        Idle,
        Queued,
        CancelPending,
        Allocating,
        Matched,
        Canceled,
        Failed,
        Expired,
        Reconnecting
    }

    [Serializable]
    public class MatchTicket
    {
        public string ticketId;
        public string state;
        public string reason;
        public long version;
        public MatchedInfoDto matchInfo;

        public MatchTicketState ParsedState => ParseState(state);

        public static MatchTicketState ParseState(string value)
        {
            if (string.IsNullOrEmpty(value)) return MatchTicketState.Unknown;
            return Enum.TryParse(value, true, out MatchTicketState parsed)
                ? parsed
                : MatchTicketState.Unknown;
        }
    }

    [Serializable]
    public class MatchCancelResult
    {
        public string result;
        public MatchTicket ticket;
    }
}

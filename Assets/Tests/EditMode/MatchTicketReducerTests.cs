using LobbyScene;
using NUnit.Framework;

public class MatchTicketReducerTests
{
    [Test]
    public void Apply_RejectsDuplicateAndOlderVersions()
    {
        var reducer = new MatchTicketReducer();

        Assert.That(reducer.Apply(Ticket("ticket-1", 3, "QUEUED")), Is.True);
        Assert.That(reducer.Apply(Ticket("ticket-1", 3, "ALLOCATING")), Is.False);
        Assert.That(reducer.Apply(Ticket("ticket-1", 2, "MATCHED")), Is.False);
        Assert.That(reducer.Current.ParsedState, Is.EqualTo(MatchTicketState.Queued));
    }

    [Test]
    public void Apply_AcceptsNewerVersion()
    {
        var reducer = new MatchTicketReducer();
        reducer.Apply(Ticket("ticket-1", 3, "QUEUED"));

        Assert.That(reducer.Apply(Ticket("ticket-1", 4, "MATCHED")), Is.True);
        Assert.That(reducer.Current.ParsedState, Is.EqualTo(MatchTicketState.Matched));
    }

    [Test]
    public void Apply_AcceptsDifferentTicketWithResetVersion()
    {
        var reducer = new MatchTicketReducer();
        reducer.Apply(Ticket("ticket-1", 8, "CANCELED"));

        Assert.That(reducer.Apply(Ticket("ticket-2", 1, "QUEUED")), Is.True);
        Assert.That(reducer.Current.ticketId, Is.EqualTo("ticket-2"));
    }

    private static MatchTicket Ticket(string id, long version, string state)
    {
        return new MatchTicket { ticketId = id, version = version, state = state };
    }
}

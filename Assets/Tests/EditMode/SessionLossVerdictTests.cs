using LobbyScene;
using NUnit.Framework;

namespace WordOnline.Tests
{
    public class SessionLossVerdictTests
    {
        [TestCase(200)]
        [TestCase(204)]
        [TestCase(404)]
        public void FromResponseCode_TreatsSuccessAndMissingTicketAsLost(int responseCode)
        {
            Assert.That(SessionLossVerdictReader.FromResponseCode(responseCode),
                Is.EqualTo(SessionLossVerdict.Lost));
        }

        [Test]
        public void FromResponseCode_TreatsConflictAsAliveSession()
        {
            Assert.That(SessionLossVerdictReader.FromResponseCode(409),
                Is.EqualTo(SessionLossVerdict.Alive));
        }

        [Test]
        public void FromResponseCode_TreatsServiceUnavailableAsDeferred()
        {
            Assert.That(SessionLossVerdictReader.FromResponseCode(503),
                Is.EqualTo(SessionLossVerdict.Deferred));
        }

        [TestCase(0)]
        [TestCase(401)]
        [TestCase(500)]
        public void FromResponseCode_TreatsUnexpectedCodesAsNotReported(int responseCode)
        {
            Assert.That(SessionLossVerdictReader.FromResponseCode(responseCode),
                Is.EqualTo(SessionLossVerdict.NotReported));
        }

        [Test]
        public void OnlyAliveKeepsTheSession()
        {
            Assert.That(SessionLossVerdict.Alive.LeavesSession(), Is.False);
            Assert.That(SessionLossVerdict.Lost.LeavesSession(), Is.True);
            Assert.That(SessionLossVerdict.Deferred.LeavesSession(), Is.True);
            Assert.That(SessionLossVerdict.NotReported.LeavesSession(), Is.True);
        }

        [Test]
        public void OnlyConfirmedLossClearsMatchState()
        {
            // 판정 보류나 신고 실패로는 매치 상태를 단정하지 않는다. 서버가 나중에 정리한다.
            Assert.That(SessionLossVerdict.Lost.ClearsMatchState(), Is.True);
            Assert.That(SessionLossVerdict.Deferred.ClearsMatchState(), Is.False);
            Assert.That(SessionLossVerdict.NotReported.ClearsMatchState(), Is.False);
            Assert.That(SessionLossVerdict.Alive.ClearsMatchState(), Is.False);
        }
    }
}

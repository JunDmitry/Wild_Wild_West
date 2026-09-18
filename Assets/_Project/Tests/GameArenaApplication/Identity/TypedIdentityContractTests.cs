using Game.Arena.Application.Identity;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Identity
{
    [TestFixture]
    public sealed class TypedIdentityContractTests
    {
        [Test]
        public void ArenaRunSourceReturnsArenaRunId()
        {
            IArenaRunIdSource source = new MonotonicArenaRunIdSource();

            ArenaRunId id = source.Allocate();

            Assert.That(id.IsNone, Is.False);
        }

        [Test]
        public void PlayerSourceReturnsPlayerId()
        {
            IPlayerIdSource source = new MonotonicPlayerIdSource();

            PlayerId id = source.Allocate();

            Assert.That(id.IsNone, Is.False);
        }

        [Test]
        public void EnemySourceReturnsEnemyId()
        {
            IEnemyIdSource source = new MonotonicEnemyIdSource();

            EnemyId id = source.Allocate();

            Assert.That(id.IsNone, Is.False);
        }
    }
}

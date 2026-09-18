using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Events
{
    [TestFixture]
    public sealed class AttackCancellationEventTests
    {
        [Test]
        public void PlayerAttackCancelledRejectsNonePlayerId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    PlayerAttackCancelled unused = new(
                            ArenaRunId.FromValue(1UL),
                            AggregateRevision.Initial,
                            PlayerId.None,
                            AttackId.None.Next(),
                            AttackCancellationCause.AttackerDefeated);
                });
        }

        [Test]
        public void EnemyAttackCancelledRejectsNoneEnemyId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    EnemyAttackCancelled unused = new(
                            ArenaRunId.FromValue(1UL),
                            AggregateRevision.Initial,
                            EnemyId.None,
                            AttackId.None.Next(),
                            AttackCancellationCause.ArenaRunTerminated);
                });
        }

        [Test]
        public void EqualCancellationEventsAreEqual()
        {
            PlayerAttackCancelled left = new(
                    ArenaRunId.FromValue(1UL),
                    AggregateRevision.Initial.Next(),
                    PlayerId.FromValue(2UL),
                    AttackId.None.Next(),
                    AttackCancellationCause.AttackerDefeated);

            PlayerAttackCancelled right = new(
                    ArenaRunId.FromValue(1UL),
                    AggregateRevision.Initial.Next(),
                    PlayerId.FromValue(2UL),
                    AttackId.None.Next(),
                    AttackCancellationCause.AttackerDefeated);

            Assert.That(left, Is.EqualTo(right));
        }
    }

}

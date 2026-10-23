using System;
using System.Collections.Generic;
using Game.Arena.Application.Notifications;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Notifications
{
    [TestFixture]
    public sealed class ArenaRunNotificationMapperTests
    {
        private static readonly ArenaRunId s_runId = ArenaRunId.FromValue(1UL);
        private static readonly AggregateRevision s_revision = AggregateRevision.Initial.Next();

        private ArenaRunNotificationMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _mapper = new ArenaRunNotificationMapper();
        }

        [Test]
        public void Map_IfNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _mapper.Map(null));
        }

        [Test]
        public void Map_WithoutEvents_ReturnsEmpty()
        {
            IReadOnlyList<IArenaRunNotification> notifications =
                _mapper.Map(Array.Empty<IArenaDomainEvent>());

            Assert.That(notifications, Is.Empty);
        }

        [Test]
        public void Map_MapsVictoryEvent()
        {
            IArenaDomainEvent[] events =
            {
                new WaveCompleted(s_runId, s_revision, WaveNumber.First),
                new ArenaRunVictorious(s_runId, s_revision),
            };

            IReadOnlyList<IArenaRunNotification> notifications = _mapper.Map(events);

            Assert.That(notifications, Has.Count.EqualTo(1));

            ArenaRunVictoryNotification notification =
                (ArenaRunVictoryNotification)notifications[0];

            Assert.That(notification.ArenaRunId, Is.EqualTo(s_runId));
            Assert.That(notification.Revision, Is.EqualTo(s_revision));
        }

        [Test]
        public void Map_MapsEnemyDamagedEvent()
        {
            EnemyId enemyId = EnemyId.FromValue(7UL);

            IArenaDomainEvent[] events =
            {
                new EnemyDamaged(s_runId, s_revision, enemyId, DamageAmount.FromPoints(20), Health.Full(30)),
            };

            IReadOnlyList<IArenaRunNotification> notifications = _mapper.Map(events);

            Assert.That(notifications, Has.Count.EqualTo(1));

            ArenaRunEnemyDamagedNotification notification =
                (ArenaRunEnemyDamagedNotification)notifications[0];

            Assert.That(notification.ArenaRunId, Is.EqualTo(s_runId));
            Assert.That(notification.Revision, Is.EqualTo(s_revision));
            Assert.That(notification.EnemyId, Is.EqualTo(enemyId));
        }

        [Test]
        public void Map_MapsOneNotificationPerDamagedEnemyInEventOrder()
        {
            EnemyId firstEnemy = EnemyId.FromValue(7UL);
            EnemyId secondEnemy = EnemyId.FromValue(8UL);

            IArenaDomainEvent[] events =
            {
                new EnemyDamaged(s_runId, s_revision, firstEnemy, DamageAmount.FromPoints(20), Health.Full(30)),
                new EnemyDamaged(s_runId, s_revision, secondEnemy, DamageAmount.FromPoints(20), Health.Full(30)),
            };

            IReadOnlyList<IArenaRunNotification> notifications = _mapper.Map(events);

            Assert.That(notifications, Has.Count.EqualTo(2));

            ArenaRunEnemyDamagedNotification first =
                (ArenaRunEnemyDamagedNotification)notifications[0];
            ArenaRunEnemyDamagedNotification second =
                (ArenaRunEnemyDamagedNotification)notifications[1];

            Assert.That(first.EnemyId, Is.EqualTo(firstEnemy));
            Assert.That(second.EnemyId, Is.EqualTo(secondEnemy));
            Assert.That(first.Revision, Is.EqualTo(s_revision));
            Assert.That(second.Revision, Is.EqualTo(s_revision));
        }

        [Test]
        public void Map_IgnoresEventsWithoutANotification()
        {
            IArenaDomainEvent[] events =
            {
                new WaveCompleted(s_runId, s_revision, WaveNumber.First),
            };

            Assert.That(_mapper.Map(events), Is.Empty);
        }
    }
}

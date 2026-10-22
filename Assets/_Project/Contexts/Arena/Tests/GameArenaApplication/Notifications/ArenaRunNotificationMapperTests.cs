using System;
using System.Collections.Generic;
using Game.Arena.Application.Notifications;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;
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
        public void Map_IgnoresNonVictoryEvents()
        {
            IArenaDomainEvent[] events =
            {
                new WaveCompleted(s_runId, s_revision, WaveNumber.First),
            };

            Assert.That(_mapper.Map(events), Is.Empty);
        }
    }
}

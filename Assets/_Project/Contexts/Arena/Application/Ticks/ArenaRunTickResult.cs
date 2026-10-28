using System;
using System.Collections.Generic;
using Game.Arena.Application.Hud;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Ticks
{
    public sealed class ArenaRunTickResult
    {
        private readonly List<IArenaDomainEvent> _eventsBuffer = new();
        private readonly List<ArenaRunTickStage> _stagesBuffer = new();
        private readonly List<IArenaRunNotification> _notificationsBuffer = new();

        internal ArenaRunTickResult()
        {
            DomainEvents = _eventsBuffer.AsReadOnly();
            ExecutedStages = _stagesBuffer.AsReadOnly();
            Notifications = _notificationsBuffer.AsReadOnly();
        }

        public ArenaRunTickResult(
            ArenaRunId arenaRunId,
            AggregateRevision finalRevision,
            ArenaRunSnapshot snapshot,
            IReadOnlyList<IArenaDomainEvent> domainEvents,
            IReadOnlyList<ArenaRunTickStage> executedStages)
            : this()
        {
            if (snapshot == null)
            {
                throw new ArgumentException(nameof(snapshot));
            }

            IArenaRunNotification[] notifications =
                CopyNotifications(new ArenaRunNotificationMapper().Map(domainEvents));

            Fill(
                arenaRunId,
                finalRevision,
                snapshot,
                new ArenaRunHudSnapshotMapper().Map(snapshot),
                domainEvents,
                executedStages,
                notifications);
        }

        public ArenaRunId ArenaRunId { get; private set; }
        public AggregateRevision FinalRevision { get; private set; }
        public ArenaRunSnapshot Snapshot { get; private set; }
        public ArenaRunHudSnapshot Hud { get; private set; }

        internal int Generation { get; private set; }

        internal bool IsPublished { get; private set; }

        internal void MarkPublished(int generation)
        {
            Generation = generation;
            IsPublished = true;
        }

        public IReadOnlyList<IArenaDomainEvent> DomainEvents { get; }
        public IReadOnlyList<ArenaRunTickStage> ExecutedStages { get; }
        public IReadOnlyList<IArenaRunNotification> Notifications { get; }

        internal void Fill(
            ArenaRunId arenaRunId,
            AggregateRevision finalRevision,
            ArenaRunSnapshot snapshot,
            ArenaRunHudSnapshot hud,
            IReadOnlyList<IArenaDomainEvent> domainEvents,
            IReadOnlyList<ArenaRunTickStage> executedStages,
            IReadOnlyList<IArenaRunNotification> notifications)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (snapshot == null)
            {
                throw new ArgumentException(nameof(snapshot));
            }

            if (snapshot.ArenaRunId != arenaRunId)
            {
                throw new ArgumentException("Snapshot belongs to another ArenaRun.", nameof(snapshot));
            }

            if (snapshot.Revision != finalRevision)
            {
                throw new ArgumentException("Snapshot revision must match the final revision.", nameof(snapshot));
            }

            if (hud == null)
            {
                throw new ArgumentNullException(nameof(hud));
            }

            if (domainEvents == null)
            {
                throw new ArgumentNullException(nameof(domainEvents));
            }

            if (executedStages == null)
            {
                throw new ArgumentNullException(nameof(executedStages));
            }

            if (notifications == null)
            {
                throw new ArgumentNullException(nameof(notifications));
            }

            _eventsBuffer.Clear();
            AggregateRevision previousRevision = AggregateRevision.Initial;

            for (int index = 0; index < domainEvents.Count; index++)
            {
                IArenaDomainEvent domainEvent = domainEvents[index];

                if (domainEvent == null)
                {
                    throw new ArgumentException("Domain events cannot contain null.", nameof(domainEvents));
                }

                if (domainEvent.ArenaRunId != arenaRunId)
                {
                    throw new ArgumentException("Domain event belongs to another ArenaRun.", nameof(domainEvents));
                }

                if (domainEvent.AggregateRevision > finalRevision)
                {
                    throw new ArgumentException("Domain event revision exceeds the final revision.", nameof(domainEvents));
                }

                if (domainEvent.AggregateRevision < previousRevision)
                {
                    throw new ArgumentException("Domain event revisions must not decrease.", nameof(domainEvents));
                }

                _eventsBuffer.Add(domainEvent);
                previousRevision = domainEvent.AggregateRevision;
            }

            _stagesBuffer.Clear();

            for (int index = 0; index < executedStages.Count; index++)
            {
                ArenaRunTickStage stage = executedStages[index];

                if ((int)stage < 0 || (int)stage > (int)ArenaRunTickStage.DefeatRestart)
                {
                    throw new ArgumentException("Executed stages contain an unknown value.", nameof(executedStages));
                }

                _stagesBuffer.Add(stage);
            }

            _notificationsBuffer.Clear();

            for (int index = 0; index < notifications.Count; index++)
            {
                _notificationsBuffer.Add(notifications[index]);
            }

            ArenaRunId = arenaRunId;
            FinalRevision = finalRevision;
            Snapshot = snapshot;
            Hud = hud;
        }

        private static IArenaRunNotification[] CopyNotifications(
            IReadOnlyList<IArenaRunNotification> notifications)
        {
            IArenaRunNotification[] copied = new IArenaRunNotification[notifications.Count];

            for (int index = 0; index < notifications.Count; index++)
            {
                copied[index] = notifications[index];
            }

            return copied;
        }
    }
}

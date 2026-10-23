using System;
using System.Collections.Generic;
using Game.Arena.Domain.Events;

namespace Game.Arena.Application.Notifications
{
    internal sealed class ArenaRunNotificationMapper
    {
        public IReadOnlyList<IArenaRunNotification> Map(
            IReadOnlyList<IArenaDomainEvent> domainEvents)
        {
            if (domainEvents == null)
            {
                throw new ArgumentNullException(nameof(domainEvents));
            }

            List<IArenaRunNotification> notifications = new();

            for (int index = 0; index < domainEvents.Count; index++)
            {
                IArenaDomainEvent domainEvent = domainEvents[index];

                if (domainEvent == null)
                {
                    throw new ArgumentException(
                        "Domain events cannot contain null.",
                        nameof(domainEvents));
                }

                if (domainEvent is ArenaRunVictorious victorious)
                {
                    notifications.Add(new ArenaRunVictoryNotification(
                        victorious.ArenaRunId,
                        victorious.AggregateRevision));
                }
                else if (domainEvent is EnemyDamaged damaged)
                {
                    notifications.Add(new ArenaRunEnemyDamagedNotification(
                        damaged.ArenaRunId,
                        damaged.AggregateRevision,
                        damaged.EnemyId));
                }
            }

            return notifications.AsReadOnly();
        }
    }
}

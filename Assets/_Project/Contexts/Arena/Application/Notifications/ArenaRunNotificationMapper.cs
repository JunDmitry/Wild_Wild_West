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
            ArenaRunNotificationPool pool = new(domainEvents.Count == 0 ? 1 : domainEvents.Count);

            pool.Fill(domainEvents, notifications);

            return notifications.AsReadOnly();
        }
    }
}

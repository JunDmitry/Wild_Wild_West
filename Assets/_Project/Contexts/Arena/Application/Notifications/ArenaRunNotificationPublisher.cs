using System;
using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Events;

namespace Game.Arena.Application.Notifications
{
    internal sealed class ArenaRunNotificationPublisher
    {
        private readonly ArenaRunNotificationMapper _mapper;
        private readonly IArenaRunNotificationSink _sink;

        public ArenaRunNotificationPublisher(IArenaRunNotificationSink sink)
        {
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _mapper = new ArenaRunNotificationMapper();
        }

        public void Publish(IReadOnlyList<IArenaDomainEvent> domainEvents)
        {
            IReadOnlyList<IArenaRunNotification> notifications = _mapper.Map(domainEvents);

            for (int index = 0; index < notifications.Count; index++)
            {
                _sink.Publish(notifications[index]);
            }
        }
    }
}

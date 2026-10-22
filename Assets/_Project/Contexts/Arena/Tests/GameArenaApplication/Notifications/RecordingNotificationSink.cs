using System;
using System.Collections.Generic;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.Ports;

namespace Game.Arena.Application.Tests.Notifications
{
    internal sealed class RecordingNotificationSink : IArenaRunNotificationSink
    {
        private readonly List<IArenaRunNotification> _published = new();

        public IReadOnlyList<IArenaRunNotification> Published => _published;

        public Exception FailWith { get; set; }

        public void Publish(IArenaRunNotification notification)
        {
            if (FailWith != null)
            {
                throw FailWith;
            }

            _published.Add(notification);
        }
    }
}

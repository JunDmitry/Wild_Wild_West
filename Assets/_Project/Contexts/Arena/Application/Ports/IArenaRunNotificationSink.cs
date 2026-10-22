using Game.Arena.Application.Notifications;

namespace Game.Arena.Application.Ports
{
    public interface IArenaRunNotificationSink
    {
        void Publish(IArenaRunNotification notification);
    }
}

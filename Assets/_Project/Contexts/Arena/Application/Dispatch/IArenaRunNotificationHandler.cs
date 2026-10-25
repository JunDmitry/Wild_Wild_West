using Game.Arena.Application.Notifications;

namespace Game.Arena.Application.Dispatch
{
    public interface IArenaRunNotificationHandler<in TNotification>
        where TNotification : IArenaRunNotification
    {
        void Handle(TNotification notification);
    }
}

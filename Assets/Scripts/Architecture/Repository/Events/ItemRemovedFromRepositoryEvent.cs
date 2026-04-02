using Assets.Scripts.Architecture.SignalBus.Interfaces;

namespace Assets.Scripts.Architecture.Repository.Events
{
    public class ItemRemovedFromRepositoryEvent<T> : IEvent
    {
        public ItemRemovedFromRepositoryEvent(T removedItem)
        {
            RemovedItem = removedItem;
        }

        public T RemovedItem { get; }
    }
}
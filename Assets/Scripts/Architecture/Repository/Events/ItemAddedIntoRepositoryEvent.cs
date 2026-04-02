using Assets.Scripts.Architecture.SignalBus.Interfaces;

namespace Assets.Scripts.Architecture.Repository.Events
{
    public class ItemAddedIntoRepositoryEvent<T> : IEvent
    {
        public ItemAddedIntoRepositoryEvent(T addedItem)
        {
            AddedItem = addedItem;
        }

        public T AddedItem { get; }
    }
}
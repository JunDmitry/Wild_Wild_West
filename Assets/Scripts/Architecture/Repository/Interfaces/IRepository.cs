// Should be interface parameters in View/Presenter
// MVP Passive View - view don't know about presenter
// MVP Active View - view know about presenter and invoke presenter method
// Presenter subscribe on model event
// DomainModelType - only properties and methods for change model properties
// ServiceModelType - don't contains fields data or contains service fields, work with models and provides interaction between models
// MVC for input. Model and view don't contain events, controller execute full work

namespace Assets.Scripts.Architecture.Repository.Interfaces
{
    public interface IRepository<TId, TInterface>
    {
        void AddItem<TClass>(TId id, TClass concreteClass) where TClass : class, TInterface;

        bool HasItem<TClass>(TId id) where TClass : class, TInterface;

        bool RemoveItem<TClass>(TId id) where TClass : class, TInterface;

        bool TryGetItem<TClass>(TId id, out TClass concreteClass) where TClass : class, TInterface;
    }
}
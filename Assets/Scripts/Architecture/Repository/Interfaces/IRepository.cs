using System.Collections.Generic;

namespace Assets.Scripts.Architecture.Repository.Interfaces
{
    public interface IRepository<TId, TInterface>
    {
        void AddItem<TClass>(TId id, TClass concreteClass) where TClass : class, TInterface;
        TInterface[] GetAll();
        bool HasItem<TClass>(TId id) where TClass : class, TInterface;

        bool RemoveItem<TClass>(TId id) where TClass : class, TInterface;

        bool TryGetItem<TClass>(TId id, out TClass concreteClass) where TClass : class, TInterface;
        IEnumerator<(TId, TInterface)> GetEnumerator();
    }
}
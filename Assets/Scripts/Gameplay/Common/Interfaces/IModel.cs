using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Architecture.Presentation.Interfaces;
using Assets.Scripts.Gameplay.Configs;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts.Gameplay.Common.Interfaces
{
    public interface IModel : IDisposable
    {
        int Id { get; }
    }

    public interface IModel<out TData> : IModel
        where TData : IModelData
    {
        TData Data { get; }
    }

    public interface IReadOnlyComponentModel : IModel
    {
        event Action<IReadOnlyComponentData> AddedComponent;
        event Action<IReadOnlyComponentData> RemovedComponent;
        event Action<IReadOnlyComponentData> ChangedComponent;

        T GetOrDefault<T>(T @default = null) where T : ComponentData;
        ComponentData GetOrDefault(int id, ComponentData @default = null);
        bool Has<T>() where T : ComponentData;
        bool Has(int id);
    }

    public interface IComponentModel : IReadOnlyComponentModel
    {
        void Add<T>(T componentData) where T : ComponentData;
        void AddOrReplace<T>(T componentData) where T : ComponentData;
        void Replace<T>(T componentData) where T : ComponentData;
        void Remove<T>() where T : ComponentData;
        void Remove(int id);
    }

    public class BaseComponentModel : IComponentModel
    {
        private readonly Dictionary<int, ComponentData> _componentByTypeId;
        
        private bool _disposed;

        public BaseComponentModel(int id, IEnumerable<IReadOnlyComponentData> initialComponents)
        {
            ThrowIf.Null(initialComponents, nameof(initialComponents));

            Id = id;
            _componentByTypeId = initialComponents.ToDictionary(c => c.TypeId, c => c.CloneDeep());
        }
        
        public event Action<IReadOnlyComponentData> AddedComponent;
        public event Action<IReadOnlyComponentData> RemovedComponent;
        public event Action<IReadOnlyComponentData> ChangedComponent;

        public int Id { get; }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (ComponentData componentData in _componentByTypeId.Values)
            {
                RemovedComponent?.Invoke(componentData);
                componentData.Dispose();
            }

            _componentByTypeId.Clear();
        }

        public void Add<T>(T componentData) where T : ComponentData
        {
            ThrowIf.Invalid(Has(TypeId<T>.Id) == false, $"This Component already contains in {GetType().Name} component model with id {Id}");

            ComponentData cloneComponent = componentData.CloneDeep();
            _componentByTypeId.Add(TypeId<T>.Id, cloneComponent);
            AddedComponent?.Invoke(cloneComponent);
        }

        public void AddOrReplace<T>(T componentData) where T : ComponentData
        {
            if (Has(TypeId<T>.Id))
                Replace(componentData);
            else
                Add(componentData);
        }

        public T GetOrDefault<T>(T @default = null) where T : ComponentData
        {
            return (T) GetOrDefault(TypeId<T>.Id, @default);
        }

        public ComponentData GetOrDefault(int id, ComponentData @default = null)
        {
            return _componentByTypeId.GetValueOrDefault(id, @default)?.CloneDeep();
        }

        public bool Has<T>() where T : ComponentData
        {
            return Has(TypeId<T>.Id);
        }

        public bool Has(int id)
        {
            return _componentByTypeId.ContainsKey(id);
        }

        public void Remove<T>() where T : ComponentData
        {
            Remove(TypeId<T>.Id);
        }

        public void Remove(int id)
        {
            if (_componentByTypeId.TryGetValue(id, out ComponentData component))
            {
                RemovedComponent?.Invoke(component);
                component.Dispose();
            }

            _componentByTypeId.Remove(id);
        }

        public void Replace<T>(T componentData) where T : ComponentData
        {
            ThrowIf.Invalid(Has(TypeId<T>.Id) == false, 
                $"Invalid {nameof(Replace)} call. Component {typeof(T).Name} does not exist in {GetType().Name} model with id {Id}");

            _componentByTypeId[TypeId<T>.Id].Dispose();
            _componentByTypeId[TypeId<T>.Id] = componentData.CloneDeep();
            ChangedComponent?.Invoke(_componentByTypeId[TypeId<T>.Id]);
        }
    }

    public interface IReadOnlyComponentData : IDisposable
    {
        int TypeId { get; }
        ComponentData CloneDeep();
    }

    [Serializable]
    public abstract class ComponentData : IReadOnlyComponentData
    {
        private static readonly ConcurrentDictionary<int, ObjectPool<ComponentData>> s_poolByTypeId = new();
        
        private int _typeId = 0;
        private bool _disposed;

        public int TypeId
        {
            get
            {
                if (_typeId == 0)
                    _typeId = TypeIdRegistry.Register(GetType());

                return _typeId;
            }
        }
        
        public static void ClearCache()
        {
            foreach (ObjectPool<ComponentData> pool in s_poolByTypeId.Values)
            {
                pool.Clear();
            }
        }

        public ComponentData CloneDeep()
        {
            IObjectPool<ComponentData> pool = GetPool();
            ComponentData clone = pool.Get();

            CopyTo(clone);

            return clone;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            IObjectPool<ComponentData> pool = GetPool();
            Reset();
            pool.Release(this);
        }

        public abstract void Reset();

        protected abstract ComponentData OnCreateItem();

        protected abstract void CopyTo(ComponentData item);

        protected virtual void OnGetItem(ComponentData componentData)
        {
            _disposed = false;
        }

        protected virtual void OnReleaseItem(ComponentData componentData)
        { }

        protected virtual void OnDestroyItem(ComponentData componentData) 
        { }

        private ObjectPool<ComponentData> GetPool()
        {
            return s_poolByTypeId.GetOrAdd(TypeId, _ => new(OnCreateItem, OnGetItem, OnReleaseItem, OnDestroyItem));
        }
    }

    public abstract class BasePresenter<TModel, TData> : IPresenter
    {
        private bool _isShow;
        private TModel _model;
        private IView<TData> _view;

        public BasePresenter(TModel model, IView<TData> view)
        {
            _model = model;
            _view = view;
        }

        public void Show()
        {
            if (_isShow)
                return;

            _isShow = true;

            OnShowing();
            _view.Show();
            OnShowed();
        }

        public void Hide()
        {
            if (_isShow == false)
                return;

            _isShow = false;

            OnHiding();
            _view.Hide();
            OnHided();
        }

        protected virtual void OnShowing()
        { }

        protected virtual void OnShowed()
        { }

        protected virtual void OnHiding()
        { }

        protected virtual void OnHided()
        { }
    }

    public interface IView
    {
        void Show();
        void Hide();
    }

    public interface IView<TData> : IView
    {
        void UpdateView(TData data);
    }

    [DisallowMultipleComponent]
    public abstract class BaseView<TData> : MonoBehaviour, IView<TData>
    {
        private bool _isShow;

        public void Show()
        {
            if (_isShow)
                return;

            _isShow = true;

            OnShowing();
            gameObject.SetActive(true);
            OnShowed();
        }

        public void Hide()
        {
            if (_isShow == false)
                return;

            _isShow = false;

            OnHiding();
            gameObject.SetActive(false);
            OnHided();
        }

        public abstract void UpdateView(TData data);

        protected virtual void OnShowing()
        { }

        protected virtual void OnShowed()
        { }

        protected virtual void OnHiding()
        { }

        protected virtual void OnHided()
        { }
    }

    public interface IComponentTag<IInData>
    {
        IEnumerator Show();
        IEnumerator Hide();
        void UpdateTag(IInData data);
    }

    // Create id with Type (enum flags) ideally for componentTag
    // IView<TData> ?? single contract??
    public abstract class ComponentTag<TInData> : MonoBehaviour, IComponentTag<TInData>
    {
        public abstract string TagName { get; }

        // Specification pattern for queries??
        public abstract IReadOnlyList<Type> RequireComponents { get; }

        public abstract IEnumerator Show();
        public abstract IEnumerator Hide();
        public abstract void UpdateTag(TInData data);
    }

    public abstract class ActiveComponentTag<TInData, TOutData> : ComponentTag<TInData>
    {
        public abstract event Action<TOutData> OnInteraction;
    }

    public static class TypeId<T>
    {
        public static readonly int Id = TypeIdRegistry.Register(typeof(T));
    }

    public class TypeIdRegistry
    {
        private const int StartId = 1;

        private readonly static object s_lock = new();
        private readonly static TypeIdRegistry s_instance = new();

        private readonly Dictionary<Type, int> _idByType;
        private int _nextId;

        private TypeIdRegistry()
        {
            _idByType = new();
            _nextId = StartId;
        }

        public static void ClearCache()
        {
            if (s_instance == null)
                return;

            s_instance._idByType.Clear();
            s_instance._nextId = StartId;
        }

        public static int Register(Type type)
        {
            lock (s_lock)
            {
                return s_instance.RegisterInternal(type);
            }
        }

        private int RegisterInternal(Type type)
        {
            if (_idByType.TryGetValue(type, out int id) == false)
            {
                id = _nextId++;
                _idByType[type] = id;
            }

            return id;
        }
    }
}
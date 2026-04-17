using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Assets.Scripts.Architecture.Presentation.Interfaces;
using Assets.Scripts.Gameplay.Configs;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Common.Interfaces
{
    public interface IModel
    {
        int Id { get; }
    }

    public interface IModel<out TData> : IModel
        where TData : IModelData
    {
        TData Data { get; }
    }

    public interface IComponentModel : IModel
    {
        T GetOrDefault<T>(T @default = null) where T : ComponentData;
        ComponentData GetOrDefault(int id, ComponentData @default = null);
        void Add<T>(T componentData) where T : ComponentData;
        void AddOrReplace<T>(T componentData) where T : ComponentData;
        void Replace<T>(T componentData) where T : ComponentData;
        void Remove<T>() where T : ComponentData;
        void Remove(int id);
        bool Has<T>() where T : ComponentData;
        bool Has(int id);
    }

    public class BaseComponentModel : IComponentModel
    {
        private readonly Dictionary<int, ComponentData> _componentByTypeId;

        public BaseComponentModel(int id, IEnumerable<IReadOnlyComponentData> initialComponents)
        {
            ThrowIf.Null(initialComponents, nameof(initialComponents));

            Id = id;
            _componentByTypeId = initialComponents.ToDictionary(c => c.TypeId, c => c.CloneDeep());
        }

        public int Id { get; }

        public void Add<T>(T componentData) where T : ComponentData
        {
            ThrowIf.Invalid(Has(TypeId<T>.Id) == false, $"This Component already contains in {GetType().Name} component model with id {Id}");

            _componentByTypeId.Add(TypeId<T>.Id, componentData.CloneDeep());
        }

        public void AddOrReplace<T>(T componentData) where T : ComponentData
        {
            _componentByTypeId[TypeId<T>.Id] = componentData.CloneDeep();
        }

        public T GetOrDefault<T>(T @default = null) where T : ComponentData
        {
            return (T) GetOrDefault(TypeId<T>.Id, @default);
        }

        public ComponentData GetOrDefault(int id, ComponentData @default = null)
        {
            return _componentByTypeId.GetValueOrDefault(id, @default);
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
            _componentByTypeId.Remove(id);
        }

        public void Replace<T>(T componentData) where T : ComponentData
        {
            ThrowIf.Invalid(Has(TypeId<T>.Id) == false, 
                $"Invalid {nameof(Replace)} call. Component {typeof(T).Name} does not exist in {GetType().Name} model with id {Id}");

            _componentByTypeId[TypeId<T>.Id] = componentData.CloneDeep();
        }
    }

    public interface IReadOnlyComponentData
    {
        int TypeId { get; }

        ComponentData CloneDeep();
    }

    [Serializable]
    public abstract class ComponentData : SafeHandle, IReadOnlyComponentData
    {
        private static readonly Dictionary<int, AutoPool<ComponentData>> s_poolByTypeId = new();

        private bool _isPooled;

        protected ComponentData()
            : base(IntPtr.Zero, true)
        {
            _isPooled = true;
        }

        public sealed override bool IsInvalid => _isPooled;
        public int TypeId => TypeIdRegistry.Register(GetType());
        
        public ComponentData CloneDeep()
        {
            ComponentData clone = GetOrCreatePoolBy(TypeId).Get();

            return clone;
        }
        
        protected sealed override bool ReleaseHandle()
        {
            AutoPool<ComponentData> pool = GetOrCreatePoolBy(TypeId);
            pool.Release(this);

            return true;
        }

        protected abstract ComponentData OnCreateItem();

        protected abstract void ConfigureClone(ComponentData item);

        protected abstract void Reset(ComponentData item);

        protected virtual void OnGetItem(ComponentData componentData)
        {
            componentData._isPooled = false;
            ConfigureClone(componentData);
        }

        protected virtual void OnReleaseItem(ComponentData componentData)
        {
            componentData._isPooled = true;
            Reset(componentData);
        }

        protected virtual void OnDestroyItem(ComponentData componentData) { }

        private AutoPool<ComponentData> GetOrCreatePoolBy(int id)
        {
            if (s_poolByTypeId.TryGetValue(id, out AutoPool<ComponentData> pool) == false)
            {
                pool = CreatePool();
                s_poolByTypeId[id] = pool;
            }

            return pool;
        }

        private AutoPool<ComponentData> CreatePool()
        {
            return new(OnCreateItem, OnGetItem, OnReleaseItem, OnDestroyItem);
        }
    }

    public class AutoPool<T> : IDisposable
        where T : class
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _create;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _initialSize;
        private readonly int _maxSize;

        public AutoPool(Func<T> create,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int initialSize = 10, 
            int maxSize = 10000)
        {
            ThrowIf.Null(create, nameof(create));
            ThrowIf.Invalid(initialSize < 0, $"{nameof(initialSize)} of AutoPool<{typeof(T).Name}> should be more or equal than zero.");
            ThrowIf.Invalid(maxSize <= 0, $"{nameof(maxSize)} of AutoPool<{typeof(T).Name}> should be positive.");

            _pool = new(initialSize);
            _create = create;
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _initialSize = initialSize;
            _maxSize = maxSize;

            Initialize();
        }

        public int TotalCount { get; private set; }
        public int InactiveCount => _pool.Count;
        public int ActiveCount => TotalCount - _pool.Count;

        public T Get()
        {
            T item;

            if (_pool.Count > 0)
                item = _pool.Pop();
            else
                item = Create();

            _onGet?.Invoke(item);

            return item;
        }

        public void Release(T item)
        {
            if (_pool.Count < _maxSize)
            {
                _onRelease?.Invoke(item);
                _pool.Push(item);
            }
            else
            {
                TotalCount--;
                _onDestroy?.Invoke(item);
            }
        }

        public void Dispose()
        {
            foreach (T item in _pool)
            {
                TotalCount--;
                _onDestroy?.Invoke(item);
            }

            _pool.Clear();
        }

        private void Initialize()
        {
            while (_pool.Count < _initialSize)
                _pool.Push(Create());
        }

        private T Create()
        {
            TotalCount++;

            return _create();
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

    // IView<TData> ?? single contract??
    public abstract class ComponentTag<TInData> : MonoBehaviour
    {
        public abstract string TagName { get; }

        // Specification pattern for queries??
        public abstract IReadOnlyList<Type> RequireComponents { get; }

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
        private readonly static object s_lock = new();
        private readonly static TypeIdRegistry s_instance;

        private readonly Dictionary<Type, int> _idByType;
        private int _nextId;

        static TypeIdRegistry()
        {
            s_instance = new TypeIdRegistry();
        }

        private TypeIdRegistry()
        {
            _idByType = new();
            _nextId = 1;
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
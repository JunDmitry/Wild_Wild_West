using System;
using System.Collections.Generic;
using System.Linq;
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
    public abstract class ComponentData : IReadOnlyComponentData
    {
        public int TypeId => TypeIdRegistry.Register(GetType());
        
        public abstract ComponentData CloneDeep();
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
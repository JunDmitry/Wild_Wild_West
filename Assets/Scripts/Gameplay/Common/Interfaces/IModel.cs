using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Architecture.Presentation.Interfaces;
using Assets.Scripts.Gameplay.Configs;
using Unity.Collections;
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
        IReadOnlyDictionary<int, IReadOnlyComponentData> StaticData { get; }

        T GetOrDefault<T>(T @default = null) where T : ComponentData;
        ComponentData GetOrDefault(int id, ComponentData @default = null);
        void Add<T>(T componentData) where T : ComponentData;
        void Replace<T>(T componentData) where T : ComponentData;
        void Remove<T>() where T : ComponentData;
        void Remove(int id);
        bool Has<T>() where T : ComponentData;
        bool Has(Type type);
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
            StaticData = initialComponents.ToDictionary(c => c.TypeId, c => c);
        }

        public int Id { get; }
        public IReadOnlyDictionary<int, IReadOnlyComponentData> StaticData { get; }

        public void Add<T>(T componentData) where T : ComponentData
        {
            ThrowIf.Invalid(Has(componentData.TypeId), $"This Component already contains in {GetType().Name} component model");

            _componentByTypeId.Add(componentData.TypeId, componentData);
        }

        public T GetOrDefault<T>(T @default = null) where T : ComponentData
        {
            return (T) GetOrDefault(typeof(T).GetId(), @default);
        }

        public ComponentData GetOrDefault(int id, ComponentData @default = null)
        {
            return _componentByTypeId.GetValueOrDefault(id, @default);
        }

        public bool Has<T>() where T : ComponentData
        {
            return Has(typeof(T));
        }

        public bool Has(Type type)
        {
            return Has(type.GetId());
        }

        public bool Has(int id)
        {
            return _componentByTypeId.ContainsKey(id);
        }

        public void Remove<T>() where T : ComponentData
        {
            Remove(typeof(T).GetId());
        }

        public void Remove(int id)
        {
            _componentByTypeId.Remove(id);
        }

        public void Replace<T>(T componentData) where T : ComponentData
        {
            _componentByTypeId[componentData.TypeId] = componentData;
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
        private int _typeId;

        public int TypeId
        {
            get
            {
                if (_typeId == 0)
                    _typeId = GetType().GetId();

                return _typeId;
            }
        }
        
        public abstract ComponentData CloneDeep();
    }

    public abstract class BasePresenter<TModel, TData> : IPresenter
    {
        private bool _isShow;
        private IModel _model;
        private IView<TData> _view;

        public BasePresenter(IModel model, IView<TData> view)
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

    public abstract class BaseView<TData> : MonoBehaviour, IView<TData>
    {
        public void Show()
        {
            OnShowing();
            gameObject.SetActive(true);
            OnShowed();
        }

        public void Hide()
        {
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

    public abstract class ComponentTag<TInData> : MonoBehaviour
    {
        public abstract string TagName { get; }
        public abstract IReadOnlyList<int> RequireComponents { get; }

        public abstract void UpdateTag(TInData data);
    }

    public abstract class ActiveComponentTag<TInData, TOutData> : ComponentTag<TInData>
    {
        public abstract event Action<TOutData> OnInteraction;
    }

    public static class TypeIdentifier
    {
        private static Dictionary<Type, int> s_idByType = new();
        private static int s_id = 1;

        public static int GetId(this Type type)
        {
            if (type.IsAbstract || type.IsInterface)
                return default;

            if (s_idByType.TryGetValue(type, out int id) == false)
            {
                id = s_id++;
                s_idByType[type] = id;
            }
            
            return id;
        }
    }
}
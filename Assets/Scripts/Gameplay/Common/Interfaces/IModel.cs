using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Architecture.Presentation.Interfaces;
using Assets.Scripts.Gameplay.Configs;
using UnityEngine;
using MyGenerated;
using Zenject;
using IInitializable = Assets.Scripts.Architecture.Lifecycle.IInitializable;
using ITickable = Assets.Scripts.Gameplay.Services.UpdateService.ITickable;
using Assets.Scripts.Common.Extensions;
using Assets.Scripts.Architecture.SignalBus.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using Assets.Scripts.Architecture.Repository.Interfaces;
using Assets.Scripts.Utility;

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
        event Action<IEntityComponent> AddedComponent;
        event Action<IEntityComponent> RemovedComponent;
        event Action<IEntityComponent> ChangedComponent;

        T GetOrDefault<T>(T @default = default) where T : struct, IEntityComponent;
        bool Has<T>() where T : struct, IEntityComponent;
        bool Has(int id);
    }

    public interface IEntityComponent 
    {
        int ComponentId { get; }
    }

    public interface IComponentModel : IReadOnlyComponentModel
    {
        void Add<T>(T componentData) where T : struct, IEntityComponent;
        void AddOrReplace<T>(T componentData) where T : struct, IEntityComponent;
        void Replace<T>(T componentData) where T : struct, IEntityComponent;
        void Remove<T>() where T : struct, IEntityComponent;
        void Remove(int id);
    }

    public class BaseComponentModel : IComponentModel
    {
        private readonly Dictionary<int, IEntityComponent> _componentByTypeId;
        
        private bool _disposed;

        public BaseComponentModel(int id, IEnumerable<IEntityComponent> initialComponents)
        {
            ThrowIf.Null(initialComponents, nameof(initialComponents));
            
            Id = id;
            _componentByTypeId = initialComponents.ToDictionary(c => c.ComponentId, c => c);
        }
        
        public event Action<IEntityComponent> AddedComponent;
        public event Action<IEntityComponent> RemovedComponent;
        public event Action<IEntityComponent> ChangedComponent;

        public int Id { get; }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (IEntityComponent componentData in _componentByTypeId.Values)
                RemovedComponent?.Invoke(componentData);

            _componentByTypeId.Clear();
        }

        public void Add<T>(T componentData) where T : struct, IEntityComponent
        {
            if (_disposed)
                return;

            ThrowIf.Invalid(Has(componentData.ComponentId) == false, $"This Component already contains in {GetType().Name} component model with id {Id}");

            _componentByTypeId.Add(componentData.ComponentId, componentData);
            AddedComponent?.Invoke(componentData);
        }

        public void AddOrReplace<T>(T componentData) where T : struct, IEntityComponent
        {
            if (_disposed)
                return;

            if (Has(componentData.ComponentId))
                Replace(componentData);
            else
                Add(componentData);
        }

        public T GetOrDefault<T>(T @default = default) where T : struct, IEntityComponent
        {
            int id = ECSComponentIds.GetEntityComponentId(typeof(T));

            return (T) GetOrDefault(id, @default);
        }

        public IEntityComponent GetOrDefault(int id, IEntityComponent @default = null)
        {
            return _disposed ? @default : _componentByTypeId.GetValueOrDefault(id, @default);
        }

        public bool Has<T>() where T : struct, IEntityComponent
        {
            return Has(ECSComponentIds.GetEntityComponentId(typeof(T)));
        }

        public bool Has(int id)
        {
            return (_disposed == false) && _componentByTypeId.ContainsKey(id);
        }

        public void Remove<T>() where T : struct, IEntityComponent
        {
            Remove(ECSComponentIds.GetEntityComponentId(typeof(T)));
        }

        public void Remove(int id)
        {
            if (_disposed)
                return;

            if (_componentByTypeId.TryGetValue(id, out IEntityComponent component))
            {
                RemovedComponent?.Invoke(component);
            }

            _componentByTypeId.Remove(id);
        }

        public void Replace<T>(T componentData) where T : struct, IEntityComponent
        {
            if (_disposed)
                return;

            ThrowIf.Invalid(Has(componentData.ComponentId) == false, 
                $"Invalid {nameof(Replace)} call. Component {typeof(T).Name} does not exist in {GetType().Name} model with id {Id}");

            _componentByTypeId[componentData.ComponentId] = componentData;
            ChangedComponent?.Invoke(_componentByTypeId[componentData.ComponentId]);
        }
    }

    public abstract class BasePresenter<TModel, TData> : IPresenter
        where TModel : class, IReadOnlyComponentModel
        where TData : class, IUpdateViewContext
    {
        private readonly ILifetimeService _lifecycleService;

        private readonly List<Func<TModel, IDisposable>> _modelSubscribes;
        private readonly List<Func<IView<TData>, IDisposable>> _viewSubscribes;
        private readonly List<Func<IDisposable>> _otherSubscribes;
        private readonly List<IDisposable> _disposables;

        private bool _isEnable;
        private bool _disposed;

        private TModel _model;
        private IView<TData> _view;

        public BasePresenter(TModel model, IView<TData> view, ILifetimeService lifetimeService)
        {
            _model = model;
            _view = view;
            _lifecycleService = lifetimeService;

            _modelSubscribes = new();
            _viewSubscribes = new();
            _otherSubscribes = new();
            _disposables = new();

            _lifecycleService.ObjectDisposed += OnObjectDispose;

            if (_lifecycleService.ScheduleInitializable(this) == false)
                Initialize();
        }

        public void Bind(IModel model, IView view)
        {
            if (model is not TModel concreteModel || view is not IView<TData> concreteView)
                throw new InvalidOperationException($"Invalid {nameof(Bind)} operation! Trying bind {nameof(IModel)} or {nameof(IView)} with incorrect type." +
                    $" Should be: model type \'{typeof(TModel).Name}\' and view type \'IView<{typeof(TData).Name}>\', " +
                    $"but was model type \'{model.GetType().Name}\' and view type \'{view.GetType().Name}\'");

            bool isEnable = _isEnable;

            Disable();

            _model = concreteModel;
            _view = concreteView;

            if (isEnable)
                Enable();
        }

        public void Enable()
        {
            if (_isEnable)
                return;

            _isEnable = true;

            OnEnabling();
            _view.Show();
            Subscribe();
            OnEnabled();
        }

        public void Disable()
        {
            if (_isEnable == false)
                return;

            _isEnable = false;

            OnDisabling();
            _view.Hide();
            Unsubscribe();
            OnDisabled();
        }

        public virtual void Initialize() 
        { }

        public void Dispose() 
        {
            if (_disposed)
                return;

            _disposed = true;
            _lifecycleService.ObjectDisposed -= OnObjectDispose;

            Disable();
            OnDispose();

            _view.Dispose();
            _model = null;
            _view = null;
            
            _lifecycleService.NotifyDisposed((IDisposable)this);
        }

        protected virtual void OnEnabling()
        { }

        protected virtual void OnEnabled()
        { }

        protected virtual void OnDisabling()
        { }

        protected virtual void OnDisabled()
        { }

        protected virtual void OnDispose()
        { }

        protected abstract TData CollectDataFromModel(TModel model);

        protected void UpdateView()
        {
            TData data = CollectDataFromModel(_model);

            if (data == null)
                return;

            _view.UpdateView(data);
        }

        protected void BindModelAutoUpdate()
        {
            RegisterModelSubscription(m =>
            {
                m.ChangedComponent += OnChangedComponent;
                m.AddedComponent += OnChangedComponent;
                m.RemovedComponent += OnChangedComponent;

                return Disposable.Create(() =>
                {
                    m.ChangedComponent -= OnChangedComponent;
                    m.AddedComponent -= OnChangedComponent;
                    m.RemovedComponent -= OnChangedComponent;
                });
            });
        }

        protected void RegisterModelSubscription(Func<TModel, IDisposable> subscription)
        {
            ThrowIf.Null(subscription, nameof(subscription));

            _modelSubscribes.Add(subscription);

            if (_isEnable && _model != null)
                _disposables.Add(subscription(_model));
        }

        protected void RegisterViewSubscription(Func<IView<TData>, IDisposable> subscription)
        {
            ThrowIf.Null(subscription, nameof(subscription));

            _viewSubscribes.Add(subscription);

            if (_isEnable && _view != null)
                _disposables.Add(subscription(_view));
        }

        protected void RegisterSubscription(Func<IDisposable> subscription)
        {
            ThrowIf.Null(subscription, nameof(subscription));

            _otherSubscribes.Add(subscription);

            if (_isEnable)
                _disposables.Add(subscription.Invoke());
        }

        private void OnChangedComponent(IEntityComponent _)
        {
            UpdateView();
        }

        private void Subscribe()
        {
            Func<TModel, IDisposable> modelFunc;
            Func<IView<TData>, IDisposable> viewFunc;
            Func<IDisposable> subscription;
            IDisposable disposable;

            for (int i = 0; i < _modelSubscribes.Count; i++)
            {
                modelFunc = _modelSubscribes[i];
                
                if (modelFunc == null)
                {
                    _modelSubscribes.RemoveAt(i);
                    i--;
                }
                else
                {
                     disposable = modelFunc(_model);

                    if (disposable != null)
                        _disposables.Add(disposable);
                }
            }

            for (int i = 0; i< _viewSubscribes.Count; i++)
            {
                viewFunc = _viewSubscribes[i];

                if (viewFunc == null)
                {
                    _viewSubscribes.RemoveAt(i);
                    i--;
                }
                else
                {
                    disposable = viewFunc(_view);

                    if (disposable != null)
                        _disposables.Add(disposable);
                }
            }

            for (int i = 0; i < _otherSubscribes.Count; i++)
            {
                subscription = _otherSubscribes[i];

                if (subscription == null)
                {
                    _otherSubscribes.RemoveAt(i);
                    i--;
                }
                else
                {
                    _disposables.Add(subscription());
                }
            }
        }

        private void Unsubscribe()
        {
            for (int i = 0; i < _disposables.Count; i++)
                _disposables[i]?.Dispose();

            _disposables.Clear();
        }

        private void OnObjectDispose(IDisposable disposable)
        {
            if (disposable != _model && disposable != _view)
                return;

            Dispose();
        }
    }

    public interface IView : IDisposable
    {
        bool IsShown { get; }

        void Show();
        void Hide();
    }

    public interface IView<TData> : IView
        where TData : class, IUpdateViewContext
    {
        void UpdateView(TData data);
    }

    [DisallowMultipleComponent]
    public abstract class BaseView<TData> : MonoBehaviour, IView<TData>
        where TData : class, IUpdateViewContext
    {
        [SerializeField] private ComponentTagsContainer _tagsContainer;

        private ILifetimeNotifier _lifetimeNotifier;
        private IRepository<int, IComponentTagsContainer> _repository;
        private bool _disposed;

        public bool IsShown => gameObject.activeSelf;

        private void OnValidate()
        {
            _tagsContainer?.Validate();
        }

        private void Start()
        {
            _tagsContainer.Initialize(gameObject.GetInstanceID());
            _repository.AddItem(gameObject.GetInstanceID(), _tagsContainer);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Show()
        {
            if (IsShown)
                return;

            OnShowing();
            gameObject.SetActive(true);

            if (_tagsContainer.HasTag<EnableComponentTagMark>() == false)
                _tagsContainer.AddTag(EnableComponentTagMark.Empty);

            OnShowed();
        }

        public void Hide()
        {
            if (IsShown == false)
                return;

            OnHiding();
            gameObject.SetActive(false);
            
            if (_tagsContainer.HasTag<EnableComponentTagMark>())
                _tagsContainer.RemoveTag<EnableComponentTagMark>();

            OnHided();
        }

        public void Dispose()
        {
            if (_disposed) 
                return;

            _disposed = true;
            OnDispose();

            _lifetimeNotifier?.NotifyDisposed(this);
            _tagsContainer.Dispose();
            _repository.RemoveItem<ComponentTagsContainer>(gameObject.GetInstanceID());
        }

        public void UpdateView(TData data)
        {
            _tagsContainer.Update(data);

            OnUpdateView(data);
        }

        protected virtual void OnUpdateView(TData data) 
        {
        }

        protected virtual void OnShowing()
        { 
        }

        protected virtual void OnShowed()
        {
        }

        protected virtual void OnHiding()
        {
        }

        protected virtual void OnHided()
        {
        }

        protected virtual void OnDispose()
        {
        }
        
        [Inject]
        private void ConstructBase(ILifetimeNotifier lifecycleNotifier, ISignalBus<IUiEvent> uiBus, IRepository<int, IComponentTagsContainer> repository)
        {
            _lifetimeNotifier = lifecycleNotifier;
            _tagsContainer.Construct(uiBus);
            _repository = repository;
        }
    }

    public interface IUpdateViewContext 
    { }

    public interface IContextConverter
    {
        Type SourceType { get; }
        Type ResultType { get; }

        IUpdateViewContext Convert(object from);
    }

    public abstract class ContextConverter<TSource, TResult> : IContextConverter
        where TSource : class
        where TResult : class, IUpdateViewContext
    {
        public Type SourceType => typeof(TSource);
        public Type ResultType => typeof(TResult);

        public IUpdateViewContext Convert(object from)
        {
            TSource source = from as TSource;

            ThrowIf.Invalid(source == null, $"Type from converter should be {SourceType.Name}, but was {from.GetType().Name}");

            return Convert(source);
        }

        public abstract TResult Convert(TSource source);
    }

    public interface IContextConverterService
    {
        K Convert<T, K>(T from)
            where T : class
            where K : class, IUpdateViewContext;
        void Reload();
    }

    public class ContextConverterService : IContextConverterService
    {
        private readonly Type _converterInterface;
        private readonly DiContainer _container;

        private Dictionary<Type, Dictionary<Type, IContextConverter>> _converterByType;

        public ContextConverterService(DiContainer container)
        {
            _converterInterface = typeof(IContextConverter);
            _container = container;

            InitializeConverters();
        }

        public K Convert<T, K>(T from)
            where T : class
            where K : class, IUpdateViewContext
        {
            Type fromType = typeof(T);
            Type toType = typeof(K);

            if (fromType == toType)
                return (K)(object) from;

            string errorMessage = $"Convert from {fromType} to {toType} does not exist. Implement converter ContextConverter<{fromType}, {toType}> or remove convert call.";

            ThrowIf.Invalid(_converterByType.TryGetValue(fromType, out Dictionary<Type, IContextConverter> target) == false, errorMessage);
            ThrowIf.Invalid(target.TryGetValue(toType, out IContextConverter converter) == false, errorMessage);

            return (K)converter.Convert(from);
        }

        public void Reload()
        {
            lock (_converterByType)
            {
                _converterByType.Clear();
                InitializeConverters();
            }
        }

        private void InitializeConverters()
        {
            _converterByType ??= new();
            IEnumerable<IContextConverter> converters = _converterInterface.FindAllNonAbstractClassAssignableFrom()
                .Select(t => (IContextConverter)_container.Instantiate(t));

            foreach (IContextConverter converter in converters)
            {
                if (_converterByType.TryGetValue(converter.SourceType, out Dictionary<Type, IContextConverter> target) == false)
                {
                    target = new();
                    _converterByType[converter.SourceType] = target;
                }

                ThrowIf.Invalid(target.ContainsKey(converter.ResultType), 
                    $"Error! Detected duplicate implementation ContextConverter<{converter.SourceType}, {converter.ResultType}>. Should be single converter type.");

                target[converter.ResultType] = converter;
            }
        }
    }

    public interface IComponentTagsContainer
    {
        int OwnerId { get; }
        bool IsDirty { get; }
        IReadOnlyList<ComponentTag> StaticTags { get; }

        void AddTag<T>(T tag) where T : ComponentTag;
        T GetTag<T>() where T : ComponentTag;
        bool HasTag<T>() where T : ComponentTag;
        bool HasTag(Type tagType);
        void RemoveTag<T>() where T : ComponentTag;
        void ReplaceTag<T>(T tag) where T : ComponentTag;
    }

    [Serializable]
    public class ComponentTagsContainer : IComponentTagsContainer, IDisposable
    {
        [SerializeReference, SubClass] private ComponentBaseTag[] _initialTags;

        private ISignalBus<IUiEvent> _signalBus;
        private int _ownerId;
        private Dictionary<Type, ComponentTag> _tags;
        private Dictionary<int, int> _requirementIds;

        public int OwnerId => _ownerId;
        public IReadOnlyList<ComponentTag> StaticTags { get; private set; }
        public bool IsDirty
        {
            get => HasTag<DirtyComponentTagMark>();
            private set
            {
                if (value && (HasTag<DirtyComponentTagMark>() == false))
                    AddTag(DirtyComponentTagMark.Empty);
                else if (value == false && HasTag<DirtyComponentTagMark>())
                    RemoveTag<DirtyComponentTagMark>();
            }
        }

        public void Dispose()
        {
            foreach (Type type in _tags.Keys.ToList())
                RemoveTag(type);

            _tags.Clear();
            _requirementIds.Clear();
            StaticTags = null;
            IsDirty = false;
        }

        public void Validate()
        {
            HashSet<Type> tags = new();

            foreach (ComponentTag tag in _initialTags)
            {
                Type type = tag.GetType();
                ThrowIf.Invalid(tags.Contains(type), $"Invalid initial tags! {nameof(ComponentTagsContainer)} cannot contains duplicate tags.");

                tags.Add(type);
            }
        }

        public void Construct(ISignalBus<IUiEvent> uiBus)
        {
            _signalBus = uiBus;
        }

        public void Initialize(int ownerId)
        {
            Validate();

            _tags = new();
            _requirementIds = new();
            IsDirty = true;
            List<ComponentTag> staticTags = new();

            foreach (ComponentTag tag in _initialTags)
            {
                AddRequirement(tag.RequireComponents);

                if (tag.IsStatic)
                    staticTags.Add(tag);
                else
                    AddTag(tag.CloneDeep());
            }

            StaticTags = staticTags.AsReadOnly();
            _ownerId = ownerId;
        }

        public T GetTag<T>() where T : ComponentTag
        {
            ThrowIf.Invalid(HasTag<T>() == false, $"You trying {nameof(GetTag)} that doesn't contains in {nameof(ComponentTagsContainer)}");

            return (T)_tags[typeof(T)];
        }

        public void AddTag<T>(T tag) where T : ComponentTag
        {
            ThrowIf.Invalid(_tags.ContainsKey(typeof(T)), $"You trying {nameof(AddTag)} that already contains in {nameof(ComponentTagsContainer)}. Use {nameof(ReplaceTag)} instead.");

            _tags[typeof(T)] = tag;

            AddRequirement(tag.RequireComponents);
            IsDirty = true;
            _signalBus.TryPublish(new AddedTagIntoComponentTagsContainer { OwnerId = _ownerId, Tag = tag });
        }

        public void RemoveTag<T>() where T : ComponentTag
        {
            RemoveTag(typeof(T));
        }

        public bool HasTag<T>() where T : ComponentTag
        {
            return HasTag(typeof(T));
        }

        public bool HasTag(Type type)
        {
            return _tags.ContainsKey(type);
        }

        public void ReplaceTag<T>(T tag) where T : ComponentTag
        {
            ThrowIf.Invalid(HasTag<T>() == false, $"You trying {nameof(ReplaceTag)} that doesn't contains in {nameof(ComponentTagsContainer)}");

            T currentTag = GetTag<T>();

            ThrowIf.Invalid(currentTag.IsStatic, "Static tags can't be replaced.");

            RemoveRequirement(currentTag.RequireComponents);
            _tags[typeof(T)] = tag;
            AddRequirement(tag.RequireComponents);
            
            IsDirty = true;
        }

        public void Update(IUpdateViewContext updateViewContext)
        {
            UpdateRequestComponentTag requestComponentTag = new(updateViewContext);

            if (HasTag<UpdateRequestComponentTag>())
                ReplaceTag(requestComponentTag);
            else
                AddTag(requestComponentTag);

            IsDirty = true;
        }

        private void RemoveTag(Type type)
        {
            ThrowIf.Invalid(_tags.TryGetValue(type, out ComponentTag tag) == false, $"You trying {nameof(RemoveTag)} that doesn't contains in {nameof(ComponentTagsContainer)}.");
            ThrowIf.Invalid(tag.IsStatic, "Static tags can't be removed if they have been added once.");

            _tags.Remove(type);

            RemoveRequirement(tag.RequireComponents);
            IsDirty = true;
            _signalBus.TryPublish(new RemovedTagIntoComponentTagsContainer { OwnerId = _ownerId, Tag = tag });
        }

        private void AddRequirement(IEnumerable<int> requirement)
        {
            foreach (int id in requirement)
            {
                if (_requirementIds.ContainsKey(id) == false)
                    _requirementIds[id] = 0;

                _requirementIds[id]++;
            }
        }

        private void RemoveRequirement(IEnumerable<int> requirement)
        {
            foreach (int id in requirement)
            {
                _requirementIds[id]--;

                if (_requirementIds[id] == 0)
                    _requirementIds.Remove(id);
            }
        }
    }

    public interface IUiEvent : IEvent
    { }

    public class AddedTagIntoComponentTagsContainer : IUiEvent
    {
        public int OwnerId;
        public ComponentTag Tag;

        public Type TagType => Tag.GetType();
    }

    public class RemovedTagIntoComponentTagsContainer : IUiEvent
    {
        public int OwnerId;
        public ComponentTag Tag;

        public Type TagType => Tag.GetType();
    }

    [Serializable]
    public abstract class ComponentTag
    {
        public abstract bool IsStatic { get; }

        public abstract IReadOnlyList<int> RequireComponents { get; }

        public abstract ComponentTag CloneDeep();
    }

    [Serializable]
    public abstract class RuntimeComponentTag : ComponentTag { }

    [Serializable]
    public abstract class ComponentBaseTag : ComponentTag { }

    public abstract class ActiveComponentTag<TOutData> : ComponentTag
    {
        public abstract event Action<TOutData> OnInteraction;
    }

    [Serializable]
    public class UpdateRequestComponentTag : RuntimeComponentTag
    {
        public UpdateRequestComponentTag(IUpdateViewContext updateViewContext)
        {
            UpdateViewContext = updateViewContext;
        }

        public IUpdateViewContext UpdateViewContext { get; }
        public override bool IsStatic => false;
        public override IReadOnlyList<int> RequireComponents => Array.Empty<int>();

        public override ComponentTag CloneDeep()
        {
            return new UpdateRequestComponentTag(UpdateViewContext);
        }
    }

    [Serializable]
    public class EnableComponentTagMark : RuntimeComponentTag
    {
        public static readonly EnableComponentTagMark Empty = new();

        public override bool IsStatic => false;
        public override IReadOnlyList<int> RequireComponents => Array.Empty<int>();

        public override ComponentTag CloneDeep()
        {
            return new EnableComponentTagMark();
        }
    }

    [Serializable]
    public class DirtyComponentTagMark : RuntimeComponentTag
    {
        public static readonly DirtyComponentTagMark Empty = new();

        public override bool IsStatic => false;
        public override IReadOnlyList<int> RequireComponents => Array.Empty<int>();

        public override ComponentTag CloneDeep()
        {
            return new DirtyComponentTagMark();
        }
    }

    public class UiWorld : IInitializable, IDisposable
    {
        private readonly ITickable _tickable;
        private readonly IFilterComponentGroupFactory _groupFactory;

        private readonly SystemsList _systemsList;
        private readonly List<IFilterComponentGroup> _filterGroups;

        public UiWorld(ITickable tickable, IFilterComponentGroupFactory groupFactory)
        {
            _tickable = tickable;
            _groupFactory = groupFactory;
            _systemsList = new();
            _filterGroups = new();
        }

        public void Initialize()
        {
            _tickable.Ticked += OnTicked;
        }

        public void AddSystem(IUiSystem uiSystem)
        {
            _systemsList.Add(uiSystem);
        }

        public IFilterComponentGroup GetGroup(TypeSet<ComponentTag> includeSet = null, TypeSet<ComponentTag> excludeSet = null)
        {
            IFilterComponentGroup group = _groupFactory.Create(includeSet, excludeSet);
            _filterGroups.Add(group);

            return group;
        }

        public void Start()
        {
            IReadOnlyList<IUiInitializable> uiInitializables = _systemsList.UiInitializables;

            for (int i = 0; i < uiInitializables.Count; i++)
                uiInitializables[i].Initialize();
        }

        private void OnTicked(float deltaTime)
        {
            IReadOnlyList<IUiExecutable> uiExecutables = _systemsList.UiExecutables;

            for (int i = 0; i < uiExecutables.Count; i++)
                uiExecutables[i].Execute(deltaTime);

            IReadOnlyList<IUiCleanUp> uiCleanUps = _systemsList.UiCleanUps;

            for (int i = 0; i < uiCleanUps.Count; i++)
                uiCleanUps[i].CleanUp();
        }

        public void Dispose()
        {
            _tickable.Ticked -= OnTicked;
            _systemsList.Dispose();

            for (int i = 0; i < _filterGroups.Count; i++)
                _filterGroups[i].Dispose();
        }
    }

    public interface IUiSystem 
    {
    }

    public interface IUiInitializable : IUiSystem
    {
        void Initialize();
    }

    public interface IUiExecutable : IUiSystem
    {
        void Execute(float deltaTime);
    }

    public interface IUiCleanUp : IUiSystem
    {
        void CleanUp();
    }

    public abstract class UiFeature : IUiInitializable, IUiExecutable, IDisposable
    {
        private readonly SystemsList _systemsList = new();

        public void Initialize()
        {
            IReadOnlyList<IUiInitializable> uiInitializables = _systemsList.UiInitializables;

            for (int i = 0; i < uiInitializables.Count; i++)
                uiInitializables[i].Initialize();
        }

        public void Execute(float deltaTime)
        {
            IReadOnlyList<IUiExecutable> uiExecutables = _systemsList.UiExecutables;

            for (int i = 0; i < uiExecutables.Count; i++)
                uiExecutables[i].Execute(deltaTime);

            IReadOnlyList<IUiCleanUp> uiCleanUps = _systemsList.UiCleanUps;

            for (int i = 0; i < uiCleanUps.Count; i++)
                uiCleanUps[i].CleanUp();
        }

        public void Dispose()
        {
            _systemsList.Dispose();
            _systemsList.Clear();
        }

        protected void Add(IUiSystem uiSystem)
        {
            _systemsList.Add(uiSystem);
        }
    }

    public class SystemsList : IDisposable
    {
        private readonly List<IUiInitializable> _uiInitializables = new();
        private readonly List<IUiExecutable> _uiExecutables = new();
        private readonly List<IUiCleanUp> _uiCleanUps = new();

        public IReadOnlyList<IUiInitializable> UiInitializables => _uiInitializables.AsReadOnly();
        public IReadOnlyList<IUiExecutable> UiExecutables => _uiExecutables.AsReadOnly();
        public IReadOnlyList<IUiCleanUp> UiCleanUps => _uiCleanUps.AsReadOnly();

        public void Dispose()
        {
            DisposeSystems(_uiInitializables);
            DisposeSystems(_uiExecutables);
            DisposeSystems(_uiCleanUps);
        }

        public void Add(IUiSystem uiSystem)
        {
            if (uiSystem is IUiInitializable initializable)
                _uiInitializables.Add(initializable);

            if (uiSystem is IUiExecutable executable)
                _uiExecutables.Add(executable);

            if (uiSystem is IUiCleanUp cleanUp)
                _uiCleanUps.Add(cleanUp);
        }

        public void Clear()
        {
            _uiInitializables.Clear();
            _uiExecutables.Clear();
            _uiCleanUps.Clear();
        }

        private void DisposeSystems(IReadOnlyList<IUiSystem> uiSystems)
        {
            foreach (IUiSystem uiSystem in uiSystems)
                if (uiSystem is IDisposable disposable)
                    disposable.Dispose();
        }
    }

    public interface IFilterComponentGroupFactory
    {
        IFilterComponentGroup Create(TypeSet<ComponentTag> includeSet, TypeSet<ComponentTag> excludeSet);
    }

    public class FilterComponentGroupFactory : IFilterComponentGroupFactory
    {
        private readonly ISignalBus<IUiEvent> _signalBus;
        private readonly IRepository<int, IComponentTagsContainer> _repository;

        public FilterComponentGroupFactory(ISignalBus<IUiEvent> signalBus, IRepository<int, IComponentTagsContainer> repository)
        {
            _signalBus = signalBus;
            _repository = repository;
        }

        public IFilterComponentGroup Create(TypeSet<ComponentTag> includeSet, TypeSet<ComponentTag> excludeSet)
        {
            return new FilterComponentGroup(_signalBus, _repository, includeSet, excludeSet);
        }
    }

    public interface IFilterComponentGroup : IDisposable
    {
        void AllOf(TypeSet<ComponentTag> typeSet);
        void NonOf(TypeSet<ComponentTag> typeSet);
        IEnumerator<IComponentTagsContainer> GetEnumerator();
    }

    public class FilterComponentGroup : IFilterComponentGroup
    {
        private readonly ISignalBus<IUiEvent> _signalBus;
        private readonly IRepository<int, IComponentTagsContainer> _repository;
        private readonly List<IComponentTagsContainer> _componentTagsContainers;

        private readonly Dictionary<int, HashSet<Type>> _idToIncludeTypes;
        private readonly Dictionary<int, HashSet<Type>> _idToExcludeTypes;

        private TypeSet<ComponentTag> _includeSet;
        private TypeSet<ComponentTag> _excludeSet;

        private readonly IDisposable _unsubscribe;

        public FilterComponentGroup(
            ISignalBus<IUiEvent> signalBus,
            IRepository<int, IComponentTagsContainer> repository,
            TypeSet<ComponentTag> includeSet = null,
            TypeSet<ComponentTag> excludeSet = null)
        {
            _signalBus = signalBus;
            _repository = repository;
            _componentTagsContainers = new();
            _idToIncludeTypes = new();
            _idToExcludeTypes = new();
            _includeSet = includeSet ?? TypeSet<ComponentTag>.Create();
            _excludeSet = excludeSet ?? TypeSet<ComponentTag>.Create();

            Initialize();

            _unsubscribe = Disposable.Combine(
                _signalBus.Subscribe<AddedTagIntoComponentTagsContainer>(OnAddedTag),
                _signalBus.Subscribe<RemovedTagIntoComponentTagsContainer>(OnRemovedTag)
            );
        }

        public void Dispose()
        {
            _unsubscribe.Dispose();
            _componentTagsContainers.Clear();
            _idToIncludeTypes.Clear();
            _idToExcludeTypes.Clear();
        }

        public void AllOf(TypeSet<ComponentTag> typeSet)
        {
            _includeSet = TypeSet<ComponentTag>.Merge(_includeSet, typeSet);

            AllOfFilter();
        }

        public void NonOf(TypeSet<ComponentTag> typeSet)
        {
            _excludeSet = TypeSet<ComponentTag>.Merge(_excludeSet, typeSet);

            NonOfFilter();
        }

        public IEnumerator<IComponentTagsContainer> GetEnumerator()
        {
            return _componentTagsContainers.GetEnumerator();
        }

        private void Initialize()
        {
            foreach ((int id, IComponentTagsContainer tagsContainer) in _repository)
            {
                HashSet<Type> includeTypes = new();
                HashSet<Type> excludeTypes = new();
                _idToIncludeTypes[id] = includeTypes;
                _idToExcludeTypes[id] = excludeTypes;

                foreach (Type includeType in _includeSet)
                {
                    if (tagsContainer.HasTag(includeType))
                        includeTypes.Add(includeType);
                }

                foreach (Type excludeType in _excludeSet)
                {
                    if (tagsContainer.HasTag(excludeType))
                        excludeTypes.Add(excludeType);
                }

                if (includeTypes.Count == _includeSet.Count && excludeTypes.Count == 0)
                    _componentTagsContainers.Add(tagsContainer);
            }
        }

        private void AllOfFilter()
        {
            OfFilter(_idToIncludeTypes, _includeSet, _includeSet.Count);
        }

        private void NonOfFilter()
        {
            OfFilter(_idToExcludeTypes, _excludeSet, 0);
        }

        private void OfFilter(Dictionary<int, HashSet<Type>> idToTypes, TypeSet<ComponentTag> set, int countToRemove)
        {
            for (int i = _componentTagsContainers.Count - 1; i >= 0; i--)
            {
                IComponentTagsContainer tagsContainer = _componentTagsContainers[i];
                HashSet<Type> excludeTypes = idToTypes[tagsContainer.OwnerId];

                foreach (Type excludeType in set)
                    if (tagsContainer.HasTag(excludeType))
                        excludeTypes.Add(excludeType);

                if (excludeTypes.Count != countToRemove)
                    _componentTagsContainers.RemoveAt(i);
            }
        }

        private void OnAddedTag(AddedTagIntoComponentTagsContainer container)
        {
            int previousIncludeCount = _idToIncludeTypes.GetValueOrDefault(container.OwnerId)?.Count ?? 0;
            int previousExcludeCount = _idToExcludeTypes.GetValueOrDefault(container.OwnerId)?.Count ?? 0;

            if (_includeSet.Contains(container.TagType))
            {
                if (_idToIncludeTypes.TryGetValue(container.OwnerId, out HashSet<Type> includeTypes) == false)
                {
                    includeTypes = new HashSet<Type>();
                    _idToIncludeTypes[container.OwnerId] = includeTypes;
                }

                includeTypes.Add(container.TagType);
            }
            else if (_excludeSet.Contains(container.TagType))
            {
                if (_idToExcludeTypes.TryGetValue(container.OwnerId, out HashSet<Type> excludeTypes) == false)
                {
                    excludeTypes = new HashSet<Type>();
                    _idToExcludeTypes[container.OwnerId] = excludeTypes;
                }

                excludeTypes.Add(container.TagType);
            }
            else
            {
                return;
            }

            int includeCount = _idToIncludeTypes.GetValueOrDefault(container.OwnerId)?.Count ?? 0;
            int excludeCount = _idToExcludeTypes.GetValueOrDefault(container.OwnerId)?.Count ?? 0;

            if (previousIncludeCount != includeCount)
            {
                if (includeCount == _includeSet.Count && excludeCount == 0
                    && _repository.TryGetItem(container.OwnerId, out IComponentTagsContainer tagsContainer))
                    _componentTagsContainers.Add(tagsContainer);
            }
            else if (previousExcludeCount != excludeCount)
            {
                if (previousExcludeCount == 0 && _repository.TryGetItem(container.OwnerId, out IComponentTagsContainer tagsContainer))
                    _componentTagsContainers.Remove(tagsContainer);
            }
        }

        private void OnRemovedTag(RemovedTagIntoComponentTagsContainer container)
        {
            int previousIncludeCount = _idToIncludeTypes.GetValueOrDefault(container.OwnerId)?.Count ?? 0;
            int previousExcludeCount = _idToExcludeTypes.GetValueOrDefault(container.OwnerId)?.Count ?? 0;

            if (_includeSet.Contains(container.TagType))
            {
                if (_idToIncludeTypes.TryGetValue(container.OwnerId, out HashSet<Type> includeTypes))
                    includeTypes.Remove(container.TagType);
            }
            else if (_excludeSet.Contains(container.TagType))
            {
                if (_idToExcludeTypes.TryGetValue(container.OwnerId, out HashSet<Type> excludeTypes))
                    excludeTypes.Remove(container.TagType);
            }
            else
            {
                return;
            }

            int includeCount = _idToIncludeTypes.GetValueOrDefault(container.OwnerId)?.Count ?? 0;
            int excludeCount = _idToExcludeTypes.GetValueOrDefault(container.OwnerId)?.Count ?? 0;

            if (previousIncludeCount != includeCount)
            {
                if (previousIncludeCount == _includeSet.Count && _repository.TryGetItem(container.OwnerId, out IComponentTagsContainer tagsContainer))
                    _componentTagsContainers.Remove(tagsContainer);
            }
            else if (previousExcludeCount != excludeCount)
            {
                if (includeCount == _includeSet.Count && excludeCount == 0
                    && _repository.TryGetItem(container.OwnerId, out IComponentTagsContainer tagsContainer))
                    _componentTagsContainers.Add(tagsContainer);
            }
        }
    }

    public class TypeSet<T>
    {
        private readonly HashSet<Type> _types;

        private TypeSet(params Type[] types)
        {
            _types = types == null ? new HashSet<Type>() : types.ToHashSet();
        }

        public int Count => _types.Count;

        public bool Contains(Type type)
        {
            return _types.Contains(type);
        }

        public void Add<T1>() 
            where T1 : T
        {
            _types.Add(typeof(T1));
        }

        public void Add<T1, T2>()
            where T1 : T
            where T2 : T
        {
            Add<T1>();
            Add<T2>();
        }

        public void Add<T1, T2, T3>()
            where T1 : T
            where T2 : T
            where T3 : T
        {
            Add<T1>();
            Add<T2>();
            Add<T3>();
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return _types.GetEnumerator();
        }

        public static TypeSet<T> Merge(TypeSet<T> first, TypeSet<T> second)
        {
            TypeSet<T> mergeSet = new();
            mergeSet._types.UnionWith(first._types);
            mergeSet._types.UnionWith(second._types);

            return mergeSet;
        }

        public static TypeSet<T> Create()
        {
            return new TypeSet<T>();
        }

        public static TypeSet<T> Create<T1>() 
            where T1 : T
        {
            return new TypeSet<T>(typeof(T1));
        }

        public static TypeSet<T> Create<T1, T2>()
            where T1 : T
            where T2 : T
        {
            return new TypeSet<T>(typeof(T1), typeof(T2));
        }

        public static TypeSet<T> Create<T1, T2, T3>()
            where T1 : T
            where T2 : T
            where T3 : T
        {
            return new TypeSet<T>(typeof(T1), typeof(T2), typeof(T3));
        }

        public static TypeSet<T> Create<T1, T2, T3, T4>()
            where T1 : T
            where T2 : T
            where T3 : T
            where T4 : T
        {
            return new TypeSet<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        public static TypeSet<T> Create<T1, T2, T3, T4, T5>()
            where T1 : T
            where T2 : T
            where T3 : T
            where T4 : T
            where T5 : T
        {
            return new TypeSet<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public static TypeSet<T> Create<T1, T2, T3, T4, T5, T6>()
            where T1 : T
            where T2 : T
            where T3 : T
            where T4 : T
            where T5 : T
            where T6 : T
        {
            return new TypeSet<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public static TypeSet<T> Create<T1, T2, T3, T4, T5, T6, T7>()
            where T1 : T
            where T2 : T
            where T3 : T
            where T4 : T
            where T5 : T
            where T6 : T
            where T7 : T
        {
            return new TypeSet<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        }

        public static TypeSet<T> Create<T1, T2, T3, T4, T5, T6, T7, T8>()
            where T1 : T
            where T2 : T
            where T3 : T
            where T4 : T
            where T5 : T
            where T6 : T
            where T7 : T
            where T8 : T
        {
            return new TypeSet<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        }

        public static TypeSet<T> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
            where T1 : T
            where T2 : T
            where T3 : T
            where T4 : T
            where T5 : T
            where T6 : T
            where T7 : T
            where T8 : T
            where T9 : T
        {
            return new TypeSet<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        }

        public static TypeSet<T> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
            where T1 : T
            where T2 : T
            where T3 : T
            where T4 : T
            where T5 : T
            where T6 : T
            where T7 : T
            where T8 : T
            where T9 : T
            where T10 : T
        {
            return new TypeSet<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        }
    }

    public interface ILifetimeNotifier
    {
        void NotifyInitialized(IInitializable initializable);
        void NotifyDisposed(IDisposable disposable);
    }

    public interface ILifetimeService : ILifetimeNotifier
    {
        event Action<IDisposable> ObjectDisposed;
        event Action<IInitializable> ObjectInitialized;

        bool ScheduleDisposable(IDisposable disposable, float initializeDelay = LifetimeService.DefaultDelay);
        bool ScheduleInitializable(IInitializable initializable, float disposeDelay = LifetimeService.DefaultDelay);
    }

    public class LifetimeService : IDisposable, ILifetimeService
    {
        public const float DefaultDelay = 1000 / 60f;

        private readonly Queue<(IInitializable, float)> _initializables;
        private readonly Queue<(IDisposable, float)> _disposables;

        private readonly ITickable _tickable;
        private readonly ITimeService _timeService;

        private bool _disposed;

        public LifetimeService(ITickable tickable, ITimeService timeService)
        {
            _initializables = new();
            _disposables = new();

            _tickable = tickable;
            _timeService = timeService;
            _tickable.Ticked += Tick;
        }

        public event Action<IInitializable> ObjectInitialized;
        public event Action<IDisposable> ObjectDisposed;

        public void NotifyInitialized(IInitializable initializable)
        {
            ObjectInitialized?.Invoke(initializable);
        }

        public void NotifyDisposed(IDisposable disposable)
        {
            ObjectDisposed?.Invoke(disposable);
        }

        public bool ScheduleInitializable(IInitializable initializable, float initializeDelay = DefaultDelay)
        {
            if (initializable == null || _disposed)
                return false;

            ThrowIf.Invalid(initializeDelay < 0, $"{nameof(initializeDelay)} should be positive. {nameof(ScheduleInitializable)}({nameof(initializable)}, {nameof(initializeDelay)})");
            _initializables.Enqueue((initializable, _timeService.Time + initializeDelay));

            return true;
        }

        public bool ScheduleDisposable(IDisposable disposable, float disposeDelay = DefaultDelay)
        {
            if (disposable == null || _disposed)
                return false;

            ThrowIf.Invalid(disposeDelay < 0, $"{nameof(disposeDelay)} should be positive. {nameof(ScheduleDisposable)}({nameof(disposable)}, {nameof(disposeDelay)})");
            _disposables.Enqueue((disposable, _timeService.Time + disposeDelay));

            return true;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _tickable.Ticked -= Tick;

            TickInitializeInternal();
            TickDisposeInternal();
        }

        private void Tick(float deltaTime)
        {
            if (_disposed)
                return;

            TickInitializeInternal();
            TickDisposeInternal();
        }

        private void TickInitializeInternal()
        {
            IInitializable initializable;
            float tickTime;

            while (_initializables.Count > 0)
            {
                (initializable, tickTime) = _initializables.Peek();

                if (tickTime > _timeService.Time)
                    break;

                _initializables.Dequeue();

                if (initializable == null)
                    continue;

                InitializeObject(initializable);
            }
        }

        private void TickDisposeInternal()
        {
            IDisposable disposable;
            float tickTime;

            while (_disposables.Count > 0)
            {
                (disposable, tickTime) = _disposables.Peek();

                if (tickTime > _timeService.Time)
                    break;

                _disposables.Dequeue();

                if (disposable == null)
                    continue;

                DisposeObject(disposable);
            }
        }

        private void InitializeObject(IInitializable initializable)
        {
            initializable.Initialize();

            ObjectInitialized?.Invoke(initializable);
        }

        private void DisposeObject(IDisposable disposable)
        {
            disposable.Dispose();

            ObjectDisposed?.Invoke(disposable);
        }
    }

    public interface ITimeService
    {
        float Time { get; }
        float DeltaTime { get; }
        float FixedDeltaTime { get; }
    }

    public class TimeService : ITimeService
    {
        public float Time => UnityEngine.Time.time;
        public float DeltaTime => UnityEngine.Time.deltaTime;
        public float FixedDeltaTime => UnityEngine.Time.fixedDeltaTime;
    }
}
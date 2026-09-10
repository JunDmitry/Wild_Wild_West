using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Architecture.Presentation.Interfaces;
using Assets.Scripts.Architecture.Repository.Interfaces;
using Assets.Scripts.Architecture.SignalBus.Interfaces;
using Assets.Scripts.Common.Extensions;
using Assets.Scripts.Gameplay.Configs;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using Assets.Scripts.Utility;
using MyGenerated;
using UnityEngine;
using Zenject;
using IInitializable = Assets.Scripts.Architecture.Lifecycle.IInitializable;
using ITickable = Assets.Scripts.Gameplay.Services.UpdateService.ITickable;

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

        T GetOrDefault<T>(T @default = default)
            where T : struct, IEntityComponent;

        bool Has<T>()
            where T : struct, IEntityComponent;

        bool Has(int id);
    }

    public interface IEntityComponent
    {
        int ComponentId { get; }
    }

    public interface IComponentModel : IReadOnlyComponentModel
    {
        void Add<T>(T componentData)
            where T : struct, IEntityComponent;

        void AddOrReplace<T>(T componentData)
            where T : struct, IEntityComponent;

        void Replace<T>(T componentData)
            where T : struct, IEntityComponent;

        void Remove<T>()
            where T : struct, IEntityComponent;

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
            {
                return;
            }

            _disposed = true;
            IEntityComponent[] copyValues = _componentByTypeId.Values.ToArray();
            _componentByTypeId.Clear();

            foreach (IEntityComponent componentData in copyValues)
            {
                RemovedComponent?.Invoke(componentData);
            }
        }

        public void Add<T>(T componentData)
            where T : struct, IEntityComponent
        {
            if (_disposed)
            {
                return;
            }

            ThrowIf.Invalid(Has(componentData.ComponentId), $"This Component already contains in {GetType().Name} component model with id {Id}");

            _componentByTypeId.Add(componentData.ComponentId, componentData);
            AddedComponent?.Invoke(componentData);
        }

        public void AddOrReplace<T>(T componentData)
            where T : struct, IEntityComponent
        {
            if (_disposed)
            {
                return;
            }

            if (Has(componentData.ComponentId))
            {
                Replace(componentData);
            }
            else
            {
                Add(componentData);
            }
        }

        public T GetOrDefault<T>(T @default = default)
            where T : struct, IEntityComponent
        {
            if (_disposed)
            {
                return @default;
            }

            int id = ECSComponentIds.GetEntityComponentId(typeof(T));

            return _componentByTypeId.TryGetValue(id, out IEntityComponent component)
                ? (T)component
                : @default;
        }

        public bool Has<T>()
            where T : struct, IEntityComponent
        {
            return Has(ECSComponentIds.GetEntityComponentId(typeof(T)));
        }

        public bool Has(int id)
        {
            return (_disposed == false) && _componentByTypeId.ContainsKey(id);
        }

        public void Remove<T>()
            where T : struct, IEntityComponent
        {
            Remove(ECSComponentIds.GetEntityComponentId(typeof(T)));
        }

        public void Remove(int id)
        {
            if (_disposed)
            {
                return;
            }

            if (_componentByTypeId.TryGetValue(id, out IEntityComponent component) == false)
            {
                return;
            }

            _componentByTypeId.Remove(id);
            RemovedComponent?.Invoke(component);
        }

        public void Replace<T>(T componentData)
            where T : struct, IEntityComponent
        {
            if (_disposed)
            {
                return;
            }

            ThrowIf.Invalid(
                Has(componentData.ComponentId) == false,
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
        private readonly ISignalBus<IUiEvent> _signalBus;

        private readonly List<Func<TModel, IDisposable>> _modelSubscribes;
        private readonly List<Func<IView<TData>, IDisposable>> _viewSubscribes;
        private readonly List<Func<IDisposable>> _otherSubscribes;

        private readonly List<IDisposable> _disposables;

        private bool _isEnable;
        private bool _disposed;

        private TModel _model;
        private IView<TData> _view;

        public BasePresenter(TModel model, IView<TData> view, ILifetimeService lifetimeService, ISignalBus<IUiEvent> signalBus)
        {
            _lifecycleService = lifetimeService;
            _signalBus = signalBus;
            _modelSubscribes = new();
            _viewSubscribes = new();
            _otherSubscribes = new();
            _disposables = new();

            Bind(model, view);

            _lifecycleService.ObjectDisposed += OnObjectDispose;

            if (_lifecycleService.ScheduleInitializable(this) == false)
            {
                Initialize();
            }
        }

        protected abstract ViewOwnership ViewOwnership { get; }

        public void Bind(IModel model, IView view)
        {
            if (model is not TModel concreteModel || view is not IView<TData> concreteView)
            {
                throw new InvalidOperationException($"Invalid {nameof(Bind)} operation! Trying bind {nameof(IModel)} or {nameof(IView)} with incorrect type." +
                    $" Should be: model type \'{typeof(TModel).Name}\' and view type \'IView<{typeof(TData).Name}>\', " +
                    $"but was model type \'{model.GetType().Name}\' and view type \'{view.GetType().Name}\'");
            }

            bool isEnable = _isEnable;

            Disable();
            Bind(concreteModel, concreteView);

            if (isEnable)
            {
                Enable();
            }
        }

        public void Enable()
        {
            if (_isEnable)
            {
                return;
            }

            _isEnable = true;

            OnEnabling();
            _view.Show();
            Subscribe();
            OnEnabled();
            UpdateView();
        }

        public void Disable()
        {
            if (_isEnable == false)
            {
                return;
            }

            _isEnable = false;

            OnDisabling();
            _view.Hide();
            Unsubscribe();
            OnDisabled();
        }

        public virtual void Initialize()
        {
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _lifecycleService.ObjectDisposed -= OnObjectDispose;

            Disable();
            OnDispose();

            if (ViewOwnership == ViewOwnership.Owned)
            {
                _view.Dispose();
            }

            _model = null;
            _view = null;

            _lifecycleService.NotifyDisposed((IDisposable)this);
        }

        protected virtual void OnEnabling()
        {
        }

        protected virtual void OnEnabled()
        {
        }

        protected virtual void OnDisabling()
        {
        }

        protected virtual void OnDisabled()
        {
        }

        protected virtual void OnDispose()
        {
        }

        protected abstract TData CollectDataFromModel(TModel model);

        protected void UpdateView()
        {
            TData data = CollectDataFromModel(_model);

            if (data == null)
            {
                return;
            }

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

        /// <summary>
        /// Subscription registration must be done once.
        /// </summary>
        /// <param name="subscription">Subscription function which returns unsubscribe disposable.</param>
        protected void RegisterModelSubscription(Func<TModel, IDisposable> subscription)
        {
            ThrowIf.Null(subscription, nameof(subscription));

            _modelSubscribes.Add(subscription);

            if (_isEnable && _model != null)
            {
                _disposables.Add(subscription(_model));
            }
        }

        /// <summary>
        /// Subscription registration must be done once.
        /// </summary>
        /// <param name="subscription">Subscription function which returns unsubscribe disposable.</param>
        protected void RegisterViewSubscription(Func<IView<TData>, IDisposable> subscription)
        {
            ThrowIf.Null(subscription, nameof(subscription));

            _viewSubscribes.Add(subscription);

            if (_isEnable && _view != null)
            {
                _disposables.Add(subscription(_view));
            }
        }

        /// <summary>
        /// Subscription registration must be done once.
        /// </summary>
        /// <typeparam name="TTag">Tag type.</typeparam>
        /// <typeparam name="TPayload">Payload type.</typeparam>
        /// <param name="handler">Handler method.</param>
        protected void RegisterInteraction<TTag, TPayload>(Action<TPayload> handler)
            where TTag : ActiveComponentTag<TPayload>
        {
            ThrowIf.Null(handler, nameof(handler));

            RegisterSubscription(() =>
            {
                return _signalBus.Subscribe(OnInteraction<TTag, TPayload>(handler));
            });
        }

        /// <summary>
        /// Subscription registration must be done once.
        /// </summary>
        /// <param name="subscription">Subscription function which returns unsubscribe disposable.</param>
        protected void RegisterSubscription(Func<IDisposable> subscription)
        {
            ThrowIf.Null(subscription, nameof(subscription));

            _otherSubscribes.Add(subscription);

            if (_isEnable)
            {
                _disposables.Add(subscription.Invoke());
            }
        }

        private static void Subscribe<TDelegate>(List<TDelegate> funcs, List<IDisposable> disposables, Func<TDelegate, IDisposable> selector)
            where TDelegate : Delegate
        {
            IDisposable disposable;
            TDelegate func;

            for (int i = 0; i < funcs.Count; i++)
            {
                func = funcs[i];

                if (func == null)
                {
                    funcs.RemoveAt(i);
                    i--;
                }
                else
                {
                    disposable = selector(func);

                    if (disposable != null)
                    {
                        disposables.Add(disposable);
                    }
                }
            }
        }

        private void OnChangedComponent(IEntityComponent component)
        {
            UpdateView();
        }

        private void Subscribe()
        {
            Subscribe(_modelSubscribes, _disposables, s => s(_model));
            Subscribe(_viewSubscribes, _disposables, s => s(_view));
            Subscribe(_otherSubscribes, _disposables, static s => s());
        }

        private void Unsubscribe()
        {
            for (int i = 0; i < _disposables.Count; i++)
            {
                _disposables[i]?.Dispose();
            }

            _disposables.Clear();
        }

        private void Bind(TModel model, IView<TData> view)
        {
            _model = model;
            _view = view;
            _view.Bind(_model.Id);
        }

        private void OnObjectDispose(IDisposable disposable)
        {
            if (_disposed)
            {
                return;
            }

            if ((ReferenceEquals(disposable, _model) == false) && (ReferenceEquals(disposable, _view) == false))
            {
                return;
            }

            Dispose();
        }

        private Action<InteractionFromComponentTag> OnInteraction<TTag, TPayload>(Action<TPayload> handler)
            where TTag : ActiveComponentTag<TPayload>
        {
            return interaction =>
            {
                if (_disposed || interaction.OwnerId != _view.Id)
                {
                    return;
                }

                if (interaction.TagType != typeof(TTag))
                {
                    return;
                }

                if (interaction.Payload is not TPayload payload)
                {
                    return;
                }

                handler(payload);
            };
        }
    }

    public interface IView : IDisposable
    {
        bool IsShown { get; }

        int Id { get; }

        int OwnerId { get; }

        void Bind(int ownerId);

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
        [SerializeField]
        private ComponentTagsContainer _tagsContainer;

        private ILifetimeNotifier _lifetimeNotifier;
        private IRepository<int, IComponentTagsContainer> _repository;

        private bool _initialized;
        private bool _disposed;

        public bool IsShown => gameObject.activeSelf;

        public int Id => gameObject.GetInstanceID();

        public int OwnerId { get; private set; }

        private void OnValidate()
        {
            _tagsContainer?.Validate();
        }

        private void Start()
        {
            EnsureInitialized();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Bind(int ownerId)
        {
            if (_disposed)
            {
                return;
            }

            OwnerId = ownerId;
        }

        public void Show()
        {
            EnsureInitialized();

            if (_tagsContainer.HasTag<RequestDisableComponentTagMark>())
            {
                float elapsedSecondsFromStart = 0;
                Dictionary<Type, float> unlockPoints = null;

                if (_tagsContainer.HasTag<TransitionLockTag>())
                {
                    TransitionLockTag transitionLockTag = _tagsContainer.GetTag<TransitionLockTag>();
                    elapsedSecondsFromStart = _tagsContainer.GetTag<TransitionLockTag>().ElapsedSecondsFromStart;
                    elapsedSecondsFromStart = transitionLockTag.ElapsedSecondsFromStart;
                    _tagsContainer.RemoveTag<TransitionLockTag>();
                }

                if (_tagsContainer.HasTag<RequestCancelDisableTransition>() == false)
                {
                    _tagsContainer.AddTag(new RequestCancelDisableTransition()
                    {
                        ElapsedSecondsFromStart = elapsedSecondsFromStart,
                        ElapsedSecondsByType = unlockPoints,
                    });
                }

                OnShow();

                return;
            }

            if (_tagsContainer.HasTag<EnableComponentTagMark>()
                || _tagsContainer.HasTag<RequestEnableComponentTagMark>())
            {
                return;
            }

            _tagsContainer.AddTag(ComponentTagMarkPool.GetMark<RequestEnableComponentTagMark>());

            OnShow();
        }

        public void Hide()
        {
            EnsureInitialized();

            if (_tagsContainer.HasTag<RequestEnableComponentTagMark>())
            {
                float elapsedSecondsFromStart = 0;
                Dictionary<Type, float> unlockPoints = null;

                if (_tagsContainer.HasTag<TransitionLockTag>())
                {
                    TransitionLockTag transitionLockTag = _tagsContainer.GetTag<TransitionLockTag>();
                    elapsedSecondsFromStart = transitionLockTag.ElapsedSecondsFromStart;
                    unlockPoints = transitionLockTag.ElapsedSecondsByType;

                    _tagsContainer.RemoveTag<TransitionLockTag>();
                }

                if (_tagsContainer.HasTag<RequestCancelEnableTransition>() == false)
                {
                    _tagsContainer.AddTag(new RequestCancelEnableTransition()
                    {
                        ElapsedSecondsFromStart = elapsedSecondsFromStart,
                        ElapsedSecondsByType = unlockPoints,
                    });
                }

                OnHide();

                return;
            }

            if ((_tagsContainer.HasTag<EnableComponentTagMark>() == false)
                || _tagsContainer.HasTag<RequestDisableComponentTagMark>())
            {
                return;
            }

            _tagsContainer.AddTag(ComponentTagMarkPool.GetMark<RequestDisableComponentTagMark>());

            OnHide();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            OnDispose();

            _lifetimeNotifier?.NotifyDisposed(this);
            _tagsContainer.Dispose();
            _repository.RemoveItem<ComponentTagsContainer>(gameObject.GetInstanceID());
        }

        public void UpdateView(TData data)
        {
            EnsureInitialized();

            _tagsContainer.Update(data);

            OnUpdateView(data);
        }

        protected virtual void OnUpdateView(TData data)
        {
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
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

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            ThrowIf.Invalid(OwnerId == 0, $"{typeof(BaseView<>).Name}<{typeof(TData).Name}> must be bound before initialization.");

            _initialized = true;
            int ownerId = Id;

            _tagsContainer.InitializeId(ownerId);
            _repository.AddItem(ownerId, _tagsContainer);
            _tagsContainer.Initialize();

            _tagsContainer.AddTag(new GameObjectComponentTag(new GameObjectWrapper(gameObject)));
        }
    }

    public sealed class GameObjectWrapper
    {
        private readonly GameObject _gameObject;

        public GameObjectWrapper(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public void SetEnable()
        {
            _gameObject.SetActive(true);
        }

        public void SetDisable()
        {
            _gameObject.SetActive(false);
        }

        public GameObjectWrapper Clone()
        {
            return new GameObjectWrapper(_gameObject);
        }
    }

    public sealed class GameObjectComponentTag : RuntimeComponentTag
    {
        public GameObjectComponentTag(GameObjectWrapper wrapper)
        {
            Wrapper = wrapper;
        }

        public override bool IsStatic => false;

        public override IReadOnlyList<int> RequireComponents => Array.Empty<int>();

        public GameObjectWrapper Wrapper { get; }

        public override ComponentTag CloneDeep()
        {
            return new GameObjectComponentTag(Wrapper.Clone());
        }
    }

    public interface IUpdateViewContext
    {
    }

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
        TTo Convert<TFrom, TTo>(TFrom from)
            where TFrom : class
            where TTo : class, IUpdateViewContext;

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

        public TTo Convert<TFrom, TTo>(TFrom from)
            where TFrom : class
            where TTo : class, IUpdateViewContext
        {
            Type fromType = typeof(TFrom);
            Type toType = typeof(TTo);

            if (fromType == toType)
            {
                return (TTo)(object)from;
            }

            string errorMessage = $"Convert from {fromType} to {toType} does not exist. Implement converter ContextConverter<{fromType}, {toType}> or remove convert call.";

            ThrowIf.Invalid(_converterByType.TryGetValue(fromType, out Dictionary<Type, IContextConverter> target) == false, errorMessage);
            ThrowIf.Invalid(target.TryGetValue(toType, out IContextConverter converter) == false, errorMessage);

            return (TTo)converter.Convert(from);
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

                ThrowIf.Invalid(
                    target.ContainsKey(converter.ResultType),
                    $"Error! Detected duplicate implementation ContextConverter<{converter.SourceType}, {converter.ResultType}>. Should be single converter type.");

                target[converter.ResultType] = converter;
            }
        }
    }

    public interface IComponentTagsContainer
    {
        int OwnerId { get; }

        IReadOnlyDictionary<Type, ComponentTag> StaticTags { get; }

        void AddTag<T>(T tag)
            where T : ComponentTag;

        T GetTag<T>()
            where T : ComponentTag;

        bool HasTag<T>()
            where T : ComponentTag;

        bool HasTag(Type tagType);

        void RemoveTag<T>()
            where T : ComponentTag;

        void ReplaceTag<T>(T tag)
            where T : ComponentTag;
    }

    [Serializable]
    public class ComponentTagsContainer : IComponentTagsContainer, IDisposable
    {
        [SerializeReference]
        [SubClass]
        private ComponentBaseTag[] _initialTags;

        private ISignalBus<IUiEvent> _signalBus;
        private int _ownerId;
        private Dictionary<Type, ComponentTag> _tags;
        private Dictionary<Type, ComponentTag> _staticTags;
        private Dictionary<int, int> _requirementIds;

        public int OwnerId => _ownerId;

        public IReadOnlyDictionary<Type, ComponentTag> StaticTags => _staticTags;

        public void Dispose()
        {
            foreach (Type type in _tags.Keys.ToList())
            {
                RemoveTag(type);
            }

            foreach (Type type in _staticTags.Keys.ToList())
            {
                ComponentTag tag = _staticTags[type];
                RemoveTag(_staticTags, tag);
            }

            _tags.Clear();
            _staticTags.Clear();
            _requirementIds.Clear();
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

        public void InitializeId(int ownerId)
        {
            _ownerId = ownerId;
        }

        public void Initialize()
        {
            Validate();

            _tags = new();
            _requirementIds = new();
            _staticTags = new();

            foreach (ComponentTag initialTag in _initialTags)
            {
                ComponentTag tag = initialTag.IsStatic ? initialTag : initialTag.CloneDeep();

                if (tag is IActiveComponentTagBinder binder)
                {
                    binder.Bind(payload => PublishInteraction(tag, payload));
                    binder.Initialize();
                }

                AddTag(tag);
            }
        }

        public T GetTag<T>()
            where T : ComponentTag
        {
            ThrowIf.Invalid(HasTag<T>() == false, $"You trying {nameof(GetTag)} that doesn't contains in {nameof(ComponentTagsContainer)}");

            Type type = typeof(T);

            return _tags.TryGetValue(type, out ComponentTag tag)
                ? (T)tag
                : (T)_staticTags[type];
        }

        public void AddTag<T>(T tag)
            where T : ComponentTag
        {
            ThrowIf.Invalid(HasTag<T>(), $"You trying {nameof(AddTag)} that already contains in {nameof(ComponentTagsContainer)}. Use {nameof(ReplaceTag)} instead.");

            AddTag((ComponentTag)tag);
        }

        public void RemoveTag<T>()
            where T : ComponentTag
        {
            RemoveTag(typeof(T));
        }

        public bool HasTag<T>()
            where T : ComponentTag
        {
            return HasTag(typeof(T));
        }

        public bool HasTag(Type type)
        {
            return _tags.ContainsKey(type) || _staticTags.ContainsKey(type);
        }

        public void ReplaceTag<T>(T tag)
            where T : ComponentTag
        {
            ThrowIf.Invalid(HasTag<T>() == false, $"You trying {nameof(ReplaceTag)} that doesn't contains in {nameof(ComponentTagsContainer)}");

            T currentTag = GetTag<T>();

            ThrowIf.Invalid(currentTag.IsStatic, "Static tags can't be replaced.");

            RemoveRequirement(currentTag.RequireComponents);
            _tags[typeof(T)] = tag;
            AddRequirement(tag.RequireComponents);
        }

        public void Update(IUpdateViewContext updateViewContext)
        {
            UpdateRequestComponentTag requestComponentTag = new(updateViewContext);

            if (HasTag<UpdateRequestComponentTag>())
            {
                ReplaceTag(requestComponentTag);
            }
            else
            {
                AddTag(requestComponentTag);
            }
        }

        private void AddTag(ComponentTag tag)
        {
            Type type = tag.GetType();

            if (tag.IsStatic)
            {
                _staticTags[type] = tag;
            }
            else
            {
                _tags[type] = tag;
            }

            AddRequirement(tag.RequireComponents);
            _signalBus.TryPublish(new AddedTagIntoComponentTagsContainer(_ownerId, tag));
        }

        private void RemoveTag(Type type)
        {
            ThrowIf.Invalid(_tags.TryGetValue(type, out ComponentTag tag) == false, $"You trying {nameof(RemoveTag)} that doesn't contains in {nameof(ComponentTagsContainer)}.");
            ThrowIf.Invalid(tag.IsStatic, "Static tags can't be removed if they have been added once.");

            RemoveTag(_tags, tag);
        }

        private void RemoveTag(Dictionary<Type, ComponentTag> tags, ComponentTag tag)
        {
            Type type = tag.GetType();
            tags.Remove(type);

            if (tag is IActiveComponentTagBinder binder)
            {
                binder.Unbind();
            }

            RemoveRequirement(tag.RequireComponents);
            _signalBus.TryPublish(new RemovedTagIntoComponentTagsContainer(_ownerId, tag));
        }

        private void AddRequirement(IEnumerable<int> requirement)
        {
            foreach (int id in requirement)
            {
                if (_requirementIds.ContainsKey(id) == false)
                {
                    _requirementIds[id] = 0;
                }

                _requirementIds[id]++;
            }
        }

        private void RemoveRequirement(IEnumerable<int> requirement)
        {
            foreach (int id in requirement)
            {
                _requirementIds[id]--;

                if (_requirementIds[id] == 0)
                {
                    _requirementIds.Remove(id);
                }
            }
        }

        private void PublishInteraction(ComponentTag componentTag, object payload)
        {
            _signalBus.TryPublish(new InteractionFromComponentTag(
                _ownerId,
                componentTag.GetType(),
                payload));
        }
    }

    public interface IUiEvent : IEvent
    {
    }

    public class AddedTagIntoComponentTagsContainer : IUiEvent
    {
        public AddedTagIntoComponentTagsContainer(int ownerId, ComponentTag tag)
        {
            OwnerId = ownerId;
            Tag = tag;
        }

        public int OwnerId { get; }

        public ComponentTag Tag { get; }

        public Type TagType => Tag.GetType();
    }

    public class RemovedTagIntoComponentTagsContainer : IUiEvent
    {
        public RemovedTagIntoComponentTagsContainer(int ownerId, ComponentTag tag)
        {
            OwnerId = ownerId;
            Tag = tag;
        }

        public int OwnerId { get; }

        public ComponentTag Tag { get; }

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
    public abstract class RuntimeComponentTag : ComponentTag
    {
    }

    [Serializable]
    public abstract class ComponentBaseTag : ComponentTag
    {
    }

    [Serializable]
    public abstract class ActiveComponentTag<TOutData> : ComponentTag, IActiveComponentTagBinder
    {
        private Action<object> _onInteraction;

        public abstract void Initialize();

        public void Bind(Action<object> handler)
        {
            _onInteraction = handler;
        }

        public void Unbind()
        {
            _onInteraction = null;
        }

        protected void RaiseInteraction(TOutData payload)
        {
            _onInteraction?.Invoke(payload);
        }
    }

    public interface IActiveComponentTagBinder
    {
        void Initialize();

        void Bind(Action<object> handler);

        void Unbind();
    }

    public class InteractionFromComponentTag : IUiEvent
    {
        public InteractionFromComponentTag(int ownerId, Type tagType, object payload)
        {
            OwnerId = ownerId;
            TagType = tagType;
            Payload = payload;
        }

        public int OwnerId { get; }

        public Type TagType { get; }

        public object Payload { get; }
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

    /// <summary>
    /// Component mark pool for <seealso cref="ComponentTagMark"/> classes.
    /// </summary>
    public static class ComponentTagMarkPool
    {
        private static readonly Dictionary<Type, ComponentTagMark> TypeToCachedMark = new();

        /// <summary>
        /// Get or (create and cache) immutable cached mark.
        /// </summary>
        /// <typeparam name="T">Mark type which contains constructor without arguments.</typeparam>
        /// <returns>Immutable cached mark.</returns>
        public static T GetMark<T>()
            where T : ComponentTagMark, new()
        {
            Type type = typeof(T);

            if (TypeToCachedMark.TryGetValue(type, out ComponentTagMark mark) == false)
            {
                mark = new T();
                TypeToCachedMark[type] = mark;
            }

            return (T)mark;
        }

        /// <summary>
        /// Clear immutable marks cache.
        /// </summary>
        public static void ClearCache()
        {
            TypeToCachedMark.Clear();
        }
    }

    public abstract class ComponentTagMark : RuntimeComponentTag
    {
        public sealed override bool IsStatic => false;

        public sealed override IReadOnlyList<int> RequireComponents => Array.Empty<int>();

        public sealed override ComponentTag CloneDeep()
        {
            return this;
        }
    }

    [Serializable]
    public class EnableComponentTagMark : ComponentTagMark
    {
    }

    [Serializable]
    public class RequestEnableComponentTagMark : ComponentTagMark
    {
    }

    [Serializable]
    public class RequestDisableComponentTagMark : ComponentTagMark
    {
    }

    [Serializable]
    public class TransitionLockTag : RuntimeComponentTag
    {
        public override bool IsStatic => false;

        public override IReadOnlyList<int> RequireComponents => Array.Empty<int>();

        public float ElapsedSecondsFromStart { get; set; }

        public HashSet<Type> Locks { get; private set; } = new HashSet<Type>();

        public Dictionary<Type, float> ElapsedSecondsByType { get; private set; } = new();

        public bool IsLocked => Locks.Count > 0;

        public override ComponentTag CloneDeep()
        {
            return new TransitionLockTag()
            {
                ElapsedSecondsFromStart = this.ElapsedSecondsFromStart,
                Locks = new HashSet<Type>(Locks),
                ElapsedSecondsByType = new Dictionary<Type, float>(ElapsedSecondsByType),
            };
        }
    }

    public class UiPipeline : Pipeline
    {
        private readonly IPipelineStageFactory _pipelineStageFactory;

        public UiPipeline(IPipelineStageFactory stageFactory)
        {
            _pipelineStageFactory = stageFactory;
        }

        public void InitializeBaseSystems()
        {
            RegisterStage((Pipeline)_pipelineStageFactory.Create<UiUpdaterFeature>(), 1);
            RegisterStage((Pipeline)_pipelineStageFactory.Create<UiEnableDisableFeature>(), 2);
        }
    }

    public sealed class UiContext : IDisposable
    {
        private readonly List<IFilterComponentGroup> _filterGroups;
        private readonly IFilterComponentGroupFactory _groupFactory;

        public UiContext(IFilterComponentGroupFactory groupFactory)
        {
            _filterGroups = new List<IFilterComponentGroup>();
            _groupFactory = groupFactory;
        }

        // Add: return created Group from cache if has, else create and return. Example XMatcher
        public IFilterComponentGroup GetGroup(TypeSet<ComponentTag> includeSet = null, TypeSet<ComponentTag> excludeSet = null)
        {
            IFilterComponentGroup group = _groupFactory.Create(includeSet, excludeSet);
            _filterGroups.Add(group);

            return group;
        }

        public void Dispose()
        {
            for (int i = 0; i < _filterGroups.Count; i++)
            {
                _filterGroups[i].Dispose();
            }
        }
    }

    public interface IPipelineStageFactory
    {
        T Create<T>()
            where T : IPipelineStage;

        T Create<T>(params object[] extraArgs)
            where T : IPipelineStage;

        IPipelineStage Create(Type type);

        IPipelineStage Create(Type type, params object[] extraArgs);
    }

    public sealed class PipelineStageFactory : IPipelineStageFactory
    {
        private readonly Type _pipelineStageType = typeof(IPipelineStage);
        private readonly DiContainer _container;

        public PipelineStageFactory(DiContainer container)
        {
            _container = container;
        }

        public T Create<T>()
            where T : IPipelineStage
        {
            return _container.Instantiate<T>();
        }

        public T Create<T>(params object[] extraArgs)
            where T : IPipelineStage
        {
            return _container.Instantiate<T>(extraArgs);
        }

        public IPipelineStage Create(Type type)
        {
            Validate(type);

            return (IPipelineStage)_container.Instantiate(type);
        }

        public IPipelineStage Create(Type type, params object[] extraArgs)
        {
            Validate(type);

            return (IPipelineStage)_container.Instantiate(type, extraArgs);
        }

        private void Validate(Type type)
        {
            ThrowIf.Invalid(_pipelineStageType.IsAssignableFrom(type) == false, $"{nameof(type)} should be assignable from {_pipelineStageType}, but was {type}.");
            ThrowIf.Invalid(IsConcreteClass(type) == false, $"{nameof(type)} should be concrete class, but was {type}.");
        }

        private bool IsConcreteClass(Type type)
        {
            return type.IsClass && (type.IsAbstract == false);
        }
    }

    internal sealed class UiUpdaterFeature : UiFeature
    {
        private readonly Type _interfaceType = typeof(IUpdateContextWithRequestSystem);
        private readonly Type _updateContextType = typeof(IUpdateViewContext);

        public override string Name => "UpdateFeature";

        public override string Description => "Update view.";

        public override int Order => 1000;

        public UiUpdaterFeature(IPipelineStageFactory systemFactory)
        {
            List<IUpdateContextWithRequestSystem> systems = CreateAllUpdateContextWithRequestSystem(systemFactory);

            SortByOrderExecution(systems);
            ValidateSystems(systems);
            AddAll(systems);

            RegisterStage(systemFactory.Create<CleanupUpdateRequestSystem>());
        }

        private List<IUpdateContextWithRequestSystem> CreateAllUpdateContextWithRequestSystem(IPipelineStageFactory systemFactory)
        {
            IEnumerable<Type> allTypes = _interfaceType.FindAllNonAbstractClassAssignableFrom();
            List<IUpdateContextWithRequestSystem> systems = new();

            foreach (Type type in allTypes)
            {
                IUpdateContextWithRequestSystem system = (IUpdateContextWithRequestSystem)systemFactory.Create(type);
                systems.Add(system);
            }

            return systems;
        }

        private void SortByOrderExecution(List<IUpdateContextWithRequestSystem> systems)
        {
            systems.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        private void ValidateSystems(List<IUpdateContextWithRequestSystem> systems)
        {
            HashSet<Type> allTypes = new(_updateContextType.FindAllNonAbstractClassAssignableFrom());

            foreach (IUpdateContextWithRequestSystem system in systems)
            {
                Type contextType = system.UpdateViewContextType;

                allTypes.Remove(contextType);
            }

            foreach (Type contextType in allTypes)
            {
                Debug.LogWarning($"For {nameof(IUpdateViewContext)} with type [{contextType}] does not exist implementation '{nameof(IUpdateContextWithRequestSystem)}'" +
                    $" and view update for this context type would be ignored. Create implementation inherited from {typeof(UpdateContextWithRequestSystem<>).Name} and release process logic.");
            }
        }

        private void AddAll(List<IUpdateContextWithRequestSystem> systems)
        {
            foreach (IPipelineStage system in systems)
            {
                RegisterStage(system);
            }
        }
    }

    internal sealed class UiEnableDisableFeature : UiFeature
    {
        public UiEnableDisableFeature(IPipelineStageFactory systemFactory)
        {
            RegisterStage(systemFactory.Create<HandleInvalidEnableDisableRequestSystem>(), 1);

            RegisterStage((Pipeline)systemFactory.Create<UiEnableFeature>(), 2);
            RegisterStage((Pipeline)systemFactory.Create<UiDisableFeature>(), 3);

            RegisterStage(systemFactory.Create<CleanupRequestSystem>(), 4);
        }

        public override string Name => "Enable&Disable View Lifecycle";

        public override string Description => "[Enable to Disable / Disable to Enable] with animate transition.";

        public override int Order => 750;
    }

    public abstract class UiLifetimeFeature<T> : UiFeature
        where T : IPipelineStage
    {
        private readonly IPipelineStageFactory _stageFactory;

        public UiLifetimeFeature(IPipelineStageFactory stageFactory)
        {
            _stageFactory = stageFactory;
        }

        protected List<T> CreateAllSystems()
        {
            IEnumerable<Type> allTypes = typeof(T).FindAllNonAbstractClassAssignableFrom();
            List<T> systems = new();

            foreach (Type type in allTypes)
            {
                T system = (T)_stageFactory.Create(type);
                systems.Add(system);
            }

            return systems;
        }

        protected void SortByOrderExecution(List<T> systems, Func<T, int> selector)
        {
            systems.Sort((a, b) => selector(a).CompareTo(selector(b)));
        }

        protected void AddAll(IReadOnlyList<IPipelineStage> stages)
        {
            foreach (IPipelineStage stage in stages)
            {
                RegisterStage(stage);
            }
        }
    }

    public sealed class UiEnableFeature : UiLifetimeFeature<IEnableWithRequestSystem>
    {
        public UiEnableFeature(IPipelineStageFactory systemFactory)
            : base(systemFactory)
        {
            List<IEnableWithRequestSystem> enableSystems = CreateAllSystems();

            SortByOrderExecution(enableSystems, t => t.Order);

            RegisterStage(systemFactory.Create<BeginEnableGameObjectFromRequestSystem>(), 1);
            AddAll(enableSystems);
            RegisterStage(systemFactory.Create<DisableToEnableWithRequestSystem>());
        }

        public override string Name => "EnableViewFeature";

        public override string Description => "Full enable cycle: EnableRequest -> Animation(if has) -> Enable -> Enable GameObject when animation complete";

        public override int Order => (int)PipelineOrder.Execute - 10;
    }

    public sealed class UiDisableFeature : UiLifetimeFeature<IDisableWithRequestSystem>
    {
        public UiDisableFeature(IPipelineStageFactory systemFactory)
            : base(systemFactory)
        {
            List<IDisableWithRequestSystem> disableSystems = CreateAllSystems();

            SortByOrderExecution(disableSystems, t => t.Order);
            AddAll(disableSystems);
            RegisterStage(systemFactory.Create<EnableToDisableWithRequestSystem>());
            RegisterStage(systemFactory.Create<CompleteDisableGameObjectSystem>());
        }

        public override string Name => "DisableViewFeature";

        public override string Description => "Full disable cycle: DisableRequest -> Animation(if has) -> Disable -> Disable GameObject when animation complete";

        public override int Order => (int)PipelineOrder.Execute - 5;
    }

    public sealed class CleanupUpdateRequestSystem : CleanupStage
    {
        private readonly List<IComponentTagsContainer> _buffer;
        private readonly IFilterComponentGroup _filterGroup;

        public CleanupUpdateRequestSystem(UiContext uiWorld)
        {
            _buffer = new();
            _filterGroup = uiWorld.GetGroup(includeSet: TypeSet<ComponentTag>.Create<
                EnableComponentTagMark,
                UpdateRequestComponentTag>());
        }

        protected override string PrefixName => "UpdateRequest";

        protected override string PrefixDescription => "Clean Update request from enabled components";

        protected override int AdditionalOrder => 10;

        protected override void Cleanup()
        {
            int index = 0;
            int elementsCount = 0;

            foreach (IComponentTagsContainer container in _filterGroup)
            {
                if (index == _buffer.Count)
                {
                    _buffer.Add(container);
                }
                else
                {
                    _buffer[index] = container;
                }

                index++;
                elementsCount++;
            }

            for (int i = 0; i < elementsCount; i++)
            {
                IComponentTagsContainer container = _buffer[i];

                container.RemoveTag<UpdateRequestComponentTag>();
            }
        }
    }

    public interface IUpdateContextWithRequestSystem : IPipelineStage
    {
        Type UpdateViewContextType { get; }
    }

    public abstract class UpdateContextWithRequestSystem<TData> : ExecuteStage, IUpdateContextWithRequestSystem
        where TData : class, IUpdateViewContext
    {
        private const int StartBufferSize = 16;

        private readonly IFilterComponentGroup _filterGroup;
        private readonly UiContext _uiWorld;

        private (TData Data, IComponentTagsContainer TagsContainer)[] _buffer;

        public UpdateContextWithRequestSystem(UiContext uiWorld)
        {
            _uiWorld = uiWorld;
            _filterGroup = GetFilterGroup(includeSet: TypeSet<ComponentTag>.Create<
                EnableComponentTagMark,
                UpdateRequestComponentTag>());
            _buffer = new (TData Data, IComponentTagsContainer TagsContainer)[StartBufferSize];
        }

        public Type UpdateViewContextType => typeof(TData);

        protected sealed override string PrefixName => $"Update{typeof(TData).Name}WithRequest";

        protected sealed override string PrefixDescription => $"Update view with {typeof(TData).Name} context from {nameof(UpdateRequestComponentTag)}";

        protected sealed override int AdditionalOrder => 75;

        public sealed override void Execute(float deltaTime)
        {
            int count = GetComponents(ref _buffer);

            for (int i = 0; i < count; i++)
            {
                TData data = _buffer[i].Data;
                IComponentTagsContainer container = _buffer[i].TagsContainer;

                Execute(data, container, deltaTime);
            }
        }

        protected IFilterComponentGroup GetFilterGroup(TypeSet<ComponentTag> includeSet = null, TypeSet<ComponentTag> excludeSet = null)
        {
            return _uiWorld.GetGroup(includeSet, excludeSet);
        }

        protected void AllOf<T>()
            where T : ComponentTag
        {
            _filterGroup.AllOf(TypeSet<ComponentTag>.Create<T>());
        }

        protected void AllOf<T1, T2>()
            where T1 : ComponentTag
            where T2 : ComponentTag
        {
            _filterGroup.AllOf(TypeSet<ComponentTag>.Create<T1, T2>());
        }

        protected void NonOf<T>()
            where T : ComponentTag
        {
            _filterGroup.NonOf(TypeSet<ComponentTag>.Create<T>());
        }

        protected void NonOf<T1, T2>()
            where T1 : ComponentTag
            where T2 : ComponentTag
        {
            _filterGroup.NonOf(TypeSet<ComponentTag>.Create<T1, T2>());
        }

        protected IEnumerable<(TData, IComponentTagsContainer)> GetComponents()
        {
            foreach (IComponentTagsContainer container in _filterGroup)
            {
                UpdateRequestComponentTag updateRequestComponentTag = container.GetTag<UpdateRequestComponentTag>();

                if (updateRequestComponentTag.UpdateViewContext is not TData data)
                {
                    continue;
                }

                yield return (data, container);
            }
        }

        protected int GetComponents(ref (TData, IComponentTagsContainer)[] buffer)
        {
            if (buffer.Length < _filterGroup.Count)
            {
                Array.Resize(ref buffer, _filterGroup.Count);
            }

            int count = 0;

            foreach (IComponentTagsContainer container in _filterGroup)
            {
                UpdateRequestComponentTag tag = container.GetTag<UpdateRequestComponentTag>();

                if (tag.UpdateViewContext is not TData data)
                {
                    continue;
                }

                buffer[count++] = (data, container);
            }

            return count;
        }

        protected abstract void Execute(TData data, IComponentTagsContainer container, float deltaTime);
    }

    public interface IEnableWithRequestSystem : IPipelineStage
    {
    }

    public interface IDisableWithRequestSystem : IPipelineStage
    {
    }

    public abstract class EnableWithRequestSystem<T> : ExecuteStage, IEnableWithRequestSystem
        where T : ComponentTag
    {
        private const int StartBufferSize = 128;

        private readonly IFilterComponentGroup _filterGroup;

        private IComponentTagsContainer[] _buffer;

        public EnableWithRequestSystem(UiContext uiWorld)
        {
            _filterGroup = uiWorld.GetGroup(
                includeSet: TypeSet<ComponentTag>.Create<RequestEnableComponentTagMark, T>(),
                excludeSet: TypeSet<ComponentTag>.Create<EnableComponentTagMark>());
            _buffer = new IComponentTagsContainer[StartBufferSize];
        }

        protected override string PrefixName => "EnableWithRequest";

        protected override string PrefixDescription => $"Enable {typeof(T).Name} {nameof(ComponentTag)} with enable animation";

        protected override int AdditionalOrder => 19;

        public sealed override void Execute(float deltaTime)
        {
            if (_buffer.Length < _filterGroup.Count)
            {
                Array.Resize(ref _buffer, _filterGroup.Count);
            }

            int count = _filterGroup.GetBuffer(_buffer);
            TransitionLockTag transitionLockTag;

            for (int i = 0; i < count; i++)
            {
                IComponentTagsContainer container = _buffer[i];
                T item = container.GetTag<T>();

                if (container.HasTag<RequestCancelEnableTransition>())
                {
                    TryCancel(container, item);

                    continue;
                }

                if (container.HasTag<TransitionLockTag>())
                {
                    transitionLockTag = container.GetTag<TransitionLockTag>();
                }
                else
                {
                    transitionLockTag = new TransitionLockTag();
                    container.AddTag(transitionLockTag);

                    OnStart(container, item);
                }

                container.WithCondition(Lock, _ => transitionLockTag.Locks.Contains(GetType()) == false);

                transitionLockTag.ElapsedSecondsFromStart += deltaTime;
                Execute(deltaTime, transitionLockTag.ElapsedSecondsFromStart, container, item);

                if (ShouldFinish(deltaTime, transitionLockTag.ElapsedSecondsFromStart, container, item))
                {
                    OnFinish(container, item);

                    container.WithCondition(Unlock, c => c.HasTag<TransitionLockTag>());
                }
            }
        }

        protected abstract void OnStart(IComponentTagsContainer container, T item);

        protected abstract void Execute(float deltaTime, float elapsedSecondsFromStart, IComponentTagsContainer container, T item);

        protected abstract bool ShouldFinish(float deltaTime, float elapsedSecondsFromStart, IComponentTagsContainer container, T item);

        protected abstract void OnCancel(float elapsedSecondsFromStart, IComponentTagsContainer container, T item);

        protected abstract void OnFinish(IComponentTagsContainer container, T item);

        private void TryCancel(IComponentTagsContainer container, T item)
        {
            RequestCancelEnableTransition requestCancel = container.GetTag<RequestCancelEnableTransition>();

            float elapsedSecondsFromStart = requestCancel.ElapsedSecondsFromStart;
            float unlockPoint = requestCancel.ElapsedSecondsByType.GetValueOrDefault(GetType(), elapsedSecondsFromStart - float.Epsilon);

            if (unlockPoint < 0)
            {
                return;
            }

            container.RemoveTag<RequestEnableComponentTagMark>();
            container.RemoveTag<RequestCancelEnableTransition>();

            container.WithCondition(c => c.RemoveTag<TransitionLockTag>(), c => c.HasTag<TransitionLockTag>());
            container.WithCondition(c => c.RemoveTag<StartedEnablingGameObjectMark>(), c => c.HasTag<StartedEnablingGameObjectMark>());

            OnCancel(elapsedSecondsFromStart, container, item);
        }

        private void Lock(IComponentTagsContainer container)
        {
            TransitionLockTag lockTag = container.GetTag<TransitionLockTag>();
            lockTag.Locks.Add(GetType());
        }

        private void Unlock(IComponentTagsContainer container)
        {
            TransitionLockTag lockTag = container.GetTag<TransitionLockTag>();
            lockTag.Locks.Remove(GetType());
            lockTag.ElapsedSecondsByType[GetType()] = lockTag.ElapsedSecondsFromStart;
        }
    }

    public abstract class DisableWithRequestSystem<T> : ExecuteStage, IDisableWithRequestSystem
        where T : ComponentTag
    {
        private const int StartBufferSize = 128;

        private readonly IFilterComponentGroup _filterGroup;

        private IComponentTagsContainer[] _buffer;

        public DisableWithRequestSystem(UiContext uiWorld)
        {
            _filterGroup = uiWorld.GetGroup(
                includeSet: TypeSet<ComponentTag>.Create<RequestDisableComponentTagMark, EnableComponentTagMark, T>());
            _buffer = new IComponentTagsContainer[StartBufferSize];
        }

        protected override string PrefixName => "DisableWithRequest";

        protected override string PrefixDescription => $"Disable {typeof(T).Name} {nameof(ComponentTag)} with disable animation";

        protected override int AdditionalOrder => 20;

        public sealed override void Execute(float deltaTime)
        {
            if (_buffer.Length < _filterGroup.Count)
            {
                Array.Resize(ref _buffer, _filterGroup.Count);
            }

            int count = _filterGroup.GetBuffer(_buffer);
            TransitionLockTag transitionLockTag;

            for (int i = 0; i < count; i++)
            {
                IComponentTagsContainer container = _buffer[i];
                T item = container.GetTag<T>();

                if (container.HasTag<RequestCancelDisableTransition>())
                {
                    Cancel(container, item);

                    continue;
                }

                if (container.HasTag<TransitionLockTag>())
                {
                    transitionLockTag = container.GetTag<TransitionLockTag>();
                }
                else
                {
                    transitionLockTag = new TransitionLockTag();
                    container.AddTag(transitionLockTag);

                    OnStart(container, item);
                }

                transitionLockTag.ElapsedSecondsFromStart += deltaTime;
                Execute(deltaTime, transitionLockTag.ElapsedSecondsFromStart, container, item);

                if (ShouldFinish(deltaTime, transitionLockTag.ElapsedSecondsFromStart, container, item))
                {
                    OnFinish(container, item);

                    container.WithCondition(c => c.RemoveTag<TransitionLockTag>(), c => c.HasTag<TransitionLockTag>());
                }
            }
        }

        protected abstract void OnStart(IComponentTagsContainer container, T item);

        protected abstract void Execute(float deltaTime, float elapsedSecondsFromStart, IComponentTagsContainer container, T item);

        protected abstract bool ShouldFinish(float deltaTime, float elapsedSecondsFromStart, IComponentTagsContainer container, T item);

        protected abstract void OnCancel(float elapsedSecondsFromStart, IComponentTagsContainer container, T item);

        protected abstract void OnFinish(IComponentTagsContainer container, T item);

        private void Cancel(IComponentTagsContainer container, T item)
        {
            float elapsedSecondsFromStart = container.GetTag<RequestCancelDisableTransition>().ElapsedSecondsFromStart;
            container.RemoveTag<RequestDisableComponentTagMark>();
            container.RemoveTag<RequestCancelDisableTransition>();

            container.WithCondition(c => c.RemoveTag<TransitionLockTag>(), c => c.HasTag<TransitionLockTag>());

            OnCancel(elapsedSecondsFromStart, container, item);
        }
    }

    public sealed class RequestCancelEnableTransition : ComponentTag
    {
        public override bool IsStatic => false;

        public override IReadOnlyList<int> RequireComponents => Array.Empty<int>();

        public float ElapsedSecondsFromStart { get; set; }

        public Dictionary<Type, float> ElapsedSecondsByType { get; set; }

        public override ComponentTag CloneDeep()
        {
            return new RequestCancelEnableTransition()
            {
                ElapsedSecondsFromStart = ElapsedSecondsFromStart,
                ElapsedSecondsByType = ElapsedSecondsByType,
            };
        }
    }

    public sealed class RequestCancelDisableTransition : ComponentTag
    {
        public override bool IsStatic => false;

        public override IReadOnlyList<int> RequireComponents => Array.Empty<int>();

        public float ElapsedSecondsFromStart { get; set; }

        public Dictionary<Type, float> ElapsedSecondsByType { get; set; }

        public override ComponentTag CloneDeep()
        {
            return new RequestCancelDisableTransition()
            {
                ElapsedSecondsFromStart = ElapsedSecondsFromStart,
                ElapsedSecondsByType = ElapsedSecondsByType,
            };
        }
    }

    public sealed class RequestCompleteDisableGameObjectMark : ComponentTagMark
    {
    }

    public sealed class StartedEnablingGameObjectMark : ComponentTagMark
    {
    }

    public sealed class EnableToDisableWithRequestSystem : ExecuteStage
    {
        private const int BufferSize = 256;

        private readonly IFilterComponentGroup _fiterGroup;
        private readonly IComponentTagsContainer[] _buffer;

        public EnableToDisableWithRequestSystem(UiContext uiWorld)
        {
            _fiterGroup = uiWorld.GetGroup(
                includeSet: TypeSet<ComponentTag>.Create<EnableComponentTagMark, RequestDisableComponentTagMark>(),
                excludeSet: TypeSet<ComponentTag>.Create<TransitionLockTag>());
            _buffer = new IComponentTagsContainer[BufferSize];
        }

        protected override string PrefixName => "EnableToDisable";

        protected override string PrefixDescription => $"Make Disable {nameof(ComponentTag)} with completed animation and add Request for disable GameObject";

        protected override int AdditionalOrder => 50;

        public override void Execute(float deltaTime)
        {
            int count = _fiterGroup.GetBuffer(_buffer);

            for (int i = 0; i < count; i++)
            {
                IComponentTagsContainer tagsContainer = _buffer[i];

                tagsContainer.RemoveTag<RequestDisableComponentTagMark>();
                tagsContainer.RemoveTag<EnableComponentTagMark>();
                tagsContainer.AddTag(ComponentTagMarkPool.GetMark<RequestCompleteDisableGameObjectMark>());
            }
        }
    }

    public sealed class DisableToEnableWithRequestSystem : ExecuteStage
    {
        private const int BufferSize = 256;

        private readonly IFilterComponentGroup _fiterGroup;
        private readonly IComponentTagsContainer[] _buffer;

        public DisableToEnableWithRequestSystem(UiContext uiWorld)
        {
            _fiterGroup = uiWorld.GetGroup(
                includeSet: TypeSet<ComponentTag>.Create<RequestEnableComponentTagMark>(),
                excludeSet: TypeSet<ComponentTag>.Create<EnableComponentTagMark, TransitionLockTag>());
            _buffer = new IComponentTagsContainer[BufferSize];
        }

        protected override string PrefixName => "DisableToEnable";

        protected override string PrefixDescription => $"Make Enable {nameof(ComponentTag)} with completed animation and add Request for enable GameObject";

        protected override int AdditionalOrder => 51;

        public override void Execute(float deltaTime)
        {
            int count = _fiterGroup.GetBuffer(_buffer);

            for (int i = 0; i < count; i++)
            {
                IComponentTagsContainer tagsContainer = _buffer[i];

                tagsContainer.RemoveTag<RequestEnableComponentTagMark>();
                tagsContainer.WithCondition(c => c.RemoveTag<StartedEnablingGameObjectMark>(), c => c.HasTag<StartedEnablingGameObjectMark>());
                tagsContainer.AddTag(ComponentTagMarkPool.GetMark<EnableComponentTagMark>());
            }
        }
    }

    public sealed class BeginEnableGameObjectFromRequestSystem : ExecuteStage
    {
        private const int StartBufferSize = 128;

        private readonly IFilterComponentGroup _filterGroup;

        private IComponentTagsContainer[] _buffer;

        public BeginEnableGameObjectFromRequestSystem(UiContext uiWorld)
        {
            _filterGroup = uiWorld.GetGroup(
                includeSet: TypeSet<ComponentTag>.Create<GameObjectComponentTag, RequestEnableComponentTagMark>(),
                excludeSet: TypeSet<ComponentTag>.Create<EnableComponentTagMark, StartedEnablingGameObjectMark>());
            _buffer = new IComponentTagsContainer[StartBufferSize];
        }

        protected override string PrefixName => "EnableGameObject";

        protected override string PrefixDescription => $"Make enable GameObject with {nameof(RequestEnableComponentTagMark)}. Requires {nameof(GameObjectComponentTag)}";

        protected override int AdditionalOrder => 100;

        public override void Execute(float deltaTime)
        {
            if (_buffer.Length < _filterGroup.Count)
            {
                Array.Resize(ref _buffer, _filterGroup.Count);
            }

            int count = _filterGroup.GetBuffer(_buffer);

            for (int i = 0; i < count; i++)
            {
                IComponentTagsContainer container = _buffer[i];

                container.GetTag<GameObjectComponentTag>().Wrapper.SetEnable();
                container.AddTag(ComponentTagMarkPool.GetMark<StartedEnablingGameObjectMark>());
            }
        }
    }

    public sealed class CompleteDisableGameObjectSystem : ExecuteStage
    {
        private const int StartBufferSize = 128;

        private readonly IFilterComponentGroup _filterGroup;

        private IComponentTagsContainer[] _buffer;

        public CompleteDisableGameObjectSystem(UiContext uiWorld)
        {
            _filterGroup = uiWorld.GetGroup(
                includeSet: TypeSet<ComponentTag>.Create<GameObjectComponentTag, RequestCompleteDisableGameObjectMark>(),
                excludeSet: TypeSet<ComponentTag>.Create<EnableComponentTagMark>());
            _buffer = new IComponentTagsContainer[StartBufferSize];
        }

        protected override string PrefixName => "DisableGameObject";

        protected override string PrefixDescription => $"Make disable GameObject with {nameof(RequestCompleteDisableGameObjectMark)}. Requires {nameof(GameObjectComponentTag)}";

        protected override int AdditionalOrder => 101;

        public override void Execute(float deltaTime)
        {
            if (_buffer.Length < _filterGroup.Count)
            {
                Array.Resize(ref _buffer, _filterGroup.Count);
            }

            int count = _filterGroup.GetBuffer(_buffer);

            for (int i = 0; i < count; i++)
            {
                IComponentTagsContainer container = _buffer[i];

                container.GetTag<GameObjectComponentTag>().Wrapper.SetDisable();
            }
        }
    }

    public sealed class CleanupRequestSystem : CleanupStage
    {
        private const int StartBufferSize = 128;

        private readonly IFilterComponentGroup _completeDisableGroup;

        private IComponentTagsContainer[] _buffer;

        public CleanupRequestSystem(UiContext uiWorld)
        {
            _completeDisableGroup = uiWorld.GetGroup(
                includeSet: TypeSet<ComponentTag>.Create<RequestCompleteDisableGameObjectMark>());
            _buffer = new IComponentTagsContainer[StartBufferSize];
        }

        protected override string PrefixName => "CompleteRequest";

        protected override string PrefixDescription => $"Cleanup {nameof(RequestCompleteDisableGameObjectMark)}";

        protected override int AdditionalOrder => 50;

        protected override void Cleanup()
        {
            HandleRemoveRequest<RequestCompleteDisableGameObjectMark>(_completeDisableGroup);
        }

        private void HandleRemoveRequest<T>(IFilterComponentGroup componetGroup)
            where T : ComponentTag
        {
            if (_buffer.Length < componetGroup.Count)
            {
                Array.Resize(ref _buffer, componetGroup.Count);
            }

            int count = componetGroup.GetBuffer(_buffer);

            for (int i = 0; i < count; i++)
            {
                IComponentTagsContainer container = _buffer[i];

                container.RemoveTag<T>();
            }
        }
    }

    public sealed class HandleInvalidEnableDisableRequestSystem : ExecuteStage
    {
        private const int BufferSize = 64;

        private readonly IFilterComponentGroup _invalidGroup;
        private readonly IComponentTagsContainer[] _buffer;

        public HandleInvalidEnableDisableRequestSystem(UiContext uiWorld)
        {
            _invalidGroup = uiWorld.GetGroup(
                TypeSet<ComponentTag>.Create<RequestEnableComponentTagMark, RequestDisableComponentTagMark>());
            _buffer = new IComponentTagsContainer[BufferSize];
        }

        protected override string PrefixName => "HandleInvalidRequest";

        protected override string PrefixDescription => $"Remove invalid requests";

        protected override int AdditionalOrder => -100;

        public override void Execute(float deltaTime)
        {
            int count = _invalidGroup.GetBuffer(_buffer);

            for (int i = 0; i < count; i++)
            {
                IComponentTagsContainer tagsContainer = _buffer[i];

                if (tagsContainer.HasTag<RequestCancelEnableTransition>() == false)
                {
                    tagsContainer.RemoveTag<RequestEnableComponentTagMark>();
                }

                if (tagsContainer.HasTag<RequestCancelDisableTransition>() == false)
                {
                    tagsContainer.RemoveTag<RequestDisableComponentTagMark>();
                }

                Debug.LogWarning($"Invalid Request[Enable/Disable]ComponentTagMark. Detected both request components at single UiEntity. Both was early suppressed and removed from entity.");
            }
        }
    }

    public abstract class PipelineStage : IPipelineStage
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract int Order { get; }

        public virtual void AfterExecute(float deltaTime)
        {
        }

        public virtual void BeforeExecute(float deltaTime)
        {
        }

        public virtual void Dispose()
        {
        }

        public virtual void Execute(float deltaTime)
        {
        }

        public virtual void Initialize()
        {
        }
    }

    public abstract class CleanupStage : PipelineStage
    {
        private string _name = null;
        private string _description = null;

        public override string Name
        {
            get
            {
                _name ??= PrefixName + "Cleanup";

                return _name;
            }
        }

        public override string Description
        {
            get
            {
                _description ??= PrefixDescription + ". Cleanup system.";

                return _description;
            }
        }

        public override int Order => (int)PipelineOrder.Cleanup + AdditionalOrder;

        protected abstract string PrefixName { get; }

        protected abstract string PrefixDescription { get; }

        protected abstract int AdditionalOrder { get; }

        public override void Execute(float deltaTime)
        {
            Cleanup();
        }

        protected abstract void Cleanup();
    }

    public abstract class ExecuteStage : PipelineStage
    {
        private string _name = null;
        private string _description = null;

        public override string Name
        {
            get
            {
                _name ??= PrefixName + "Execute";

                return _name;
            }
        }

        public override string Description
        {
            get
            {
                _description ??= PrefixDescription + ". Execute system.";

                return _description;
            }
        }

        public override int Order => (int)PipelineOrder.Execute + AdditionalOrder;

        protected abstract string PrefixName { get; }

        protected abstract string PrefixDescription { get; }

        protected abstract int AdditionalOrder { get; }

        public abstract override void Execute(float deltaTime);
    }

    public abstract class InitializableStage : PipelineStage
    {
        private string _name = null;
        private string _description = null;

        public override string Name
        {
            get
            {
                _name ??= PrefixName + "Initializable";

                return _name;
            }
        }

        public override string Description
        {
            get
            {
                _description ??= PrefixDescription + ". Initialize system.";

                return _description;
            }
        }

        public override int Order => (int)PipelineOrder.Initialize + AdditionalOrder;

        protected abstract string PrefixName { get; }

        protected abstract string PrefixDescription { get; }

        protected abstract int AdditionalOrder { get; }

        public sealed override void BeforeExecute(float deltaTime)
        {
        }

        public sealed override void Execute(float deltaTime)
        {
        }

        public sealed override void AfterExecute(float deltaTime)
        {
        }
    }

    public abstract class UiFeature : Pipeline, IPipelineStage
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract int Order { get; }

        public void Initialize()
        {
            Start();
        }

        public virtual void BeforeExecute(float deltaTime)
        {
        }

        public void Execute(float deltaTime)
        {
            Tick(deltaTime);
        }

        public virtual void AfterExecute(float deltaTime)
        {
        }
    }

    public enum PipelineOrder
    {
        NoOverride = -1,
        None = 0,
        Initialize = 4000,
        Execute = 400000,
        Cleanup = 4000000,
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
        int Count { get; }

        void AllOf(TypeSet<ComponentTag> typeSet);

        void NonOf(TypeSet<ComponentTag> typeSet);

        IEnumerator<IComponentTagsContainer> GetEnumerator();

        int GetBuffer(IComponentTagsContainer[] buffer);
    }

    public class FilterComponentGroup : IFilterComponentGroup
    {
        private readonly ISignalBus<IUiEvent> _signalBus;
        private readonly IRepository<int, IComponentTagsContainer> _repository;

        private readonly HashSet<int> _containerIds;
        private readonly List<IComponentTagsContainer> _componentTagsContainers;

        private readonly Dictionary<int, HashSet<Type>> _idToIncludeTypes;
        private readonly Dictionary<int, HashSet<Type>> _idToExcludeTypes;

        private readonly IDisposable _unsubscribe;

        private TypeSet<ComponentTag> _includeSet;
        private TypeSet<ComponentTag> _excludeSet;

        public FilterComponentGroup(
            ISignalBus<IUiEvent> signalBus,
            IRepository<int, IComponentTagsContainer> repository,
            TypeSet<ComponentTag> includeSet = null,
            TypeSet<ComponentTag> excludeSet = null)
        {
            _repository = repository;
            _signalBus = signalBus;

            _containerIds = new();
            _componentTagsContainers = new();

            _idToIncludeTypes = new();
            _idToExcludeTypes = new();

            _includeSet = includeSet ?? TypeSet<ComponentTag>.Create();
            _excludeSet = excludeSet ?? TypeSet<ComponentTag>.Create();

            BuildCache();

            _unsubscribe = Disposable.Combine(
                _signalBus.Subscribe<AddedTagIntoComponentTagsContainer>(OnAddedTag),
                _signalBus.Subscribe<RemovedTagIntoComponentTagsContainer>(OnRemovedTag));
        }

        public int Count => _componentTagsContainers.Count;

        public void Dispose()
        {
            _unsubscribe.Dispose();
            _componentTagsContainers.Clear();
            _idToIncludeTypes.Clear();
            _idToExcludeTypes.Clear();
        }

        /// <summary>
        /// It is recommended to use it only during initialization.
        /// </summary>
        /// <param name="typeSet">Type set to include.</param>
        public void AllOf(TypeSet<ComponentTag> typeSet)
        {
            _includeSet = TypeSet<ComponentTag>.Merge(_includeSet, typeSet);

            BuildCache();
        }

        /// <summary>
        /// It is recommended to use it only during initialization.
        /// </summary>
        /// <param name="typeSet">Type set to include.</param>
        public void NonOf(TypeSet<ComponentTag> typeSet)
        {
            _excludeSet = TypeSet<ComponentTag>.Merge(_excludeSet, typeSet);

            BuildCache();
        }

        public IEnumerator<IComponentTagsContainer> GetEnumerator()
        {
            return _componentTagsContainers.GetEnumerator();
        }

        public int GetBuffer(IComponentTagsContainer[] buffer)
        {
            int count = 0;

            foreach (IComponentTagsContainer tag in _componentTagsContainers)
            {
                if (buffer.Length == count)
                {
                    break;
                }

                buffer[count] = tag;
                count++;
            }

            return count;
        }

        private void BuildCache()
        {
            _containerIds.Clear();
            _componentTagsContainers.Clear();
            _idToIncludeTypes.Clear();
            _idToExcludeTypes.Clear();

            foreach ((int id, IComponentTagsContainer tagsContainer) in _repository)
            {
                HashSet<Type> includeTypes = new();
                HashSet<Type> excludeTypes = new();
                _idToIncludeTypes[id] = includeTypes;
                _idToExcludeTypes[id] = excludeTypes;

                foreach (Type includeType in _includeSet)
                {
                    if (tagsContainer.HasTag(includeType))
                    {
                        includeTypes.Add(includeType);
                    }
                }

                foreach (Type excludeType in _excludeSet)
                {
                    if (tagsContainer.HasTag(excludeType))
                    {
                        excludeTypes.Add(excludeType);
                    }
                }

                if (includeTypes.Count == _includeSet.Count && excludeTypes.Count == 0)
                {
                    AddContainer(tagsContainer);
                }
                else
                {
                    RemoveContainer(tagsContainer);
                }
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
                {
                    AddContainer(tagsContainer);
                }
            }
            else if (previousExcludeCount != excludeCount)
            {
                if (previousExcludeCount == 0 && _repository.TryGetItem(container.OwnerId, out IComponentTagsContainer tagsContainer))
                {
                    RemoveContainer(tagsContainer);
                }
            }
        }

        private void OnRemovedTag(RemovedTagIntoComponentTagsContainer container)
        {
            int previousIncludeCount = _idToIncludeTypes.GetValueOrDefault(container.OwnerId)?.Count ?? 0;
            int previousExcludeCount = _idToExcludeTypes.GetValueOrDefault(container.OwnerId)?.Count ?? 0;

            if (_includeSet.Contains(container.TagType))
            {
                if (_idToIncludeTypes.TryGetValue(container.OwnerId, out HashSet<Type> includeTypes))
                {
                    includeTypes.Remove(container.TagType);
                }
            }
            else if (_excludeSet.Contains(container.TagType))
            {
                if (_idToExcludeTypes.TryGetValue(container.OwnerId, out HashSet<Type> excludeTypes))
                {
                    excludeTypes.Remove(container.TagType);
                }
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
                {
                    RemoveContainer(tagsContainer);
                }
            }
            else if (previousExcludeCount != excludeCount)
            {
                if (includeCount == _includeSet.Count && excludeCount == 0
                    && _repository.TryGetItem(container.OwnerId, out IComponentTagsContainer tagsContainer))
                {
                    AddContainer(tagsContainer);
                }
            }
        }

        private void AddContainer(IComponentTagsContainer container)
        {
            if (_containerIds.Contains(container.OwnerId))
            {
                return;
            }

            _containerIds.Add(container.OwnerId);
            _componentTagsContainers.Add(container);
        }

        private void RemoveContainer(IComponentTagsContainer container)
        {
            if (_containerIds.Contains(container.OwnerId) == false)
            {
                return;
            }

            _containerIds.Remove(container.OwnerId);
            _componentTagsContainers.Remove(container);
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
        public const float DefaultDelay = 1 / 60f;

        private readonly PriorityQueue<IInitializable, float> _initializables;
        private readonly PriorityQueue<IDisposable, float> _disposables;

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
            {
                return false;
            }

            ThrowIf.Invalid(initializeDelay < 0, $"{nameof(initializeDelay)} should be positive. {nameof(ScheduleInitializable)}({nameof(initializable)}, {nameof(initializeDelay)})");
            _initializables.Enqueue(initializable, _timeService.Time + initializeDelay);

            return true;
        }

        public bool ScheduleDisposable(IDisposable disposable, float disposeDelay = DefaultDelay)
        {
            if (disposable == null || _disposed)
            {
                return false;
            }

            ThrowIf.Invalid(disposeDelay < 0, $"{nameof(disposeDelay)} should be positive. {nameof(ScheduleDisposable)}({nameof(disposable)}, {nameof(disposeDelay)})");
            _disposables.Enqueue(disposable, _timeService.Time + disposeDelay);

            return true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _tickable.Ticked -= Tick;

            TickInitializeInternal();
            TickDisposeInternal();
        }

        private void Tick(float deltaTime)
        {
            if (_disposed)
            {
                return;
            }

            TickInitializeInternal();
            TickDisposeInternal();
        }

        private void TickInitializeInternal()
        {
            while (_initializables.Count > 0)
            {
                (IInitializable item, float time) = _initializables.PeekFirstPair();

                if (time > _timeService.Time)
                {
                    break;
                }

                _initializables.DequeueFirst();

                if (item == null)
                {
                    continue;
                }

                InitializeObject(item);
            }
        }

        private void TickDisposeInternal()
        {
            while (_disposables.Count > 0)
            {
                (IDisposable item, float time) = _disposables.PeekFirstPair();

                if (time > _timeService.Time)
                {
                    break;
                }

                _disposables.DequeueFirst();

                if (item == null)
                {
                    continue;
                }

                DisposeObject(item);
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

    public enum ViewOwnership
    {
        /// <summary>
        /// No value
        /// </summary>
        None = 0,

        /// <summary>
        /// Presenter owns the view
        /// </summary>
        Owned = 1,

        /// <summary>
        /// Unity owns the view
        /// </summary>
        Borrowed = 2,
    }

    public interface IPipelineStageProvider
    {
        IPipelineStage GetPipelineStage(IPipelineContext context);
    }

    public interface IPipelineStage : IDisposable
    {
        string Name { get; }

        string Description { get; }

        int Order { get; }

        void Initialize();

        void BeforeExecute(float deltaTime);

        void Execute(float deltaTime);

        void AfterExecute(float deltaTime);
    }

    public interface IPipelineContext
    {
    }

    public interface IPipeline : IDisposable
    {
        event Action<string> Staging;

        event Action<string> Staged;

        IDisposable RegisterStage(IPipelineStageProvider stageProvider, int overrideOrder = (int)PipelineOrder.NoOverride);

        IDisposable RegisterStage(IPipelineStage stage, int overrideOrder = (int)PipelineOrder.NoOverride);

        IDisposable RegisterStage(IPipeline childPipeline, int order = (int)PipelineOrder.Execute);

        void Start();

        void Tick(float deltaTime);
    }

    public class PipelineContext : IPipelineContext
    {
        private readonly Pipeline _pipeline;

        public PipelineContext(Pipeline pipeline)
        {
            _pipeline = pipeline;
        }
    }

    public class PipelineStageAdapter : IPipelineStage
    {
        private readonly IPipeline _pipeline;

        public PipelineStageAdapter(IPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public string Name => _pipeline.GetType().Name;

        public string Description => "Adapted Pipeline for PipelineStage";

        public int Order => 0;

        public void AfterExecute(float deltaTime)
        {
        }

        public void BeforeExecute(float deltaTime)
        {
        }

        public void Dispose()
        {
            _pipeline.Dispose();
        }

        public void Execute(float deltaTime)
        {
            _pipeline.Tick(deltaTime);
        }

        public void Initialize()
        {
            _pipeline.Start();
        }
    }

    public abstract class Pipeline : IPipeline
    {
        private readonly List<PipelineStageEntry> _pipelineStages;
        private readonly Scheduler<IPipelineStage> _initializeScheduler;

        private Pipeline _parent;
        private HashSet<Pipeline> _visitedHash = new();

        private bool _started;
        private bool _disposed;

        public Pipeline()
        {
            _pipelineStages = new List<PipelineStageEntry>();
            _initializeScheduler = new Scheduler<IPipelineStage>(stage => stage.Initialize());
        }

        public event Action<string> Staging;

        public event Action<string> Staged;

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _started = false;

            for (int i = 0; i < _pipelineStages.Count; i++)
            {
                _pipelineStages[i].Stage.Dispose();
            }

            _pipelineStages.Clear();
        }

        public IDisposable RegisterStage(IPipelineStageProvider stageProvider, int overrideOrder = (int)PipelineOrder.NoOverride)
        {
            if (_disposed)
            {
                return Disposable.Create(() => { });
            }

            IPipelineContext pipelineContext = GetPipelineContext();
            IPipelineStage stage = stageProvider.GetPipelineStage(pipelineContext);

            return RegisterStage(stage, overrideOrder);
        }

        public IDisposable RegisterStage(IPipelineStage stage, int overrideOrder = (int)PipelineOrder.NoOverride)
        {
            if (_disposed)
            {
                return Disposable.Create(() => { });
            }

            int order = overrideOrder == (int)PipelineOrder.NoOverride ? stage.Order : overrideOrder;
            PipelineStageEntry entry = new PipelineStageEntry(stage, order);
            int index = FindLessOrEqualIndex(order);

            _pipelineStages.Insert(index, entry);

            if (_started)
            {
                _initializeScheduler.Schedule(entry.Stage);
            }

            return Disposable.Create(() =>
            {
                entry.Stage.Dispose();
                _pipelineStages.Remove(entry);
            });
        }

        public IDisposable RegisterStage(IPipeline pipeline, int order = (int)PipelineOrder.Execute)
        {
            if (_disposed)
            {
                return Disposable.Create(() => { });
            }

            IPipelineStage stage = new PipelineStageAdapter(pipeline);

            if (pipeline is Pipeline child)
            {
                child._parent = this;
            }

            return RegisterStage(stage, order);
        }

        public void Start()
        {
            if (_started || _disposed)
            {
                return;
            }

            _started = true;

            for (int i = 0; i < _pipelineStages.Count; i++)
            {
                _pipelineStages[i].Stage.Initialize();
            }
        }

        public void Tick(float deltaTime)
        {
            if (_disposed || (_started == false))
            {
                return;
            }

            _initializeScheduler.Work();

            for (int i = 0; i < _pipelineStages.Count; i++)
            {
                IPipelineStage stage = _pipelineStages[i].Stage;

                Publish(pipeline => pipeline.Staging?.Invoke(stage.Name));

                stage.BeforeExecute(deltaTime);
                stage.Execute(deltaTime);
                stage.AfterExecute(deltaTime);

                Publish(pipeline => pipeline.Staged?.Invoke(stage.Name));
            }
        }

        protected virtual IPipelineContext GetPipelineContext()
        {
            return new PipelineContext(this);
        }

        private void Publish(Action<Pipeline> @event)
        {
            Pipeline pipeline = this;

            while (pipeline != null)
            {
                if (_visitedHash.Contains(pipeline))
                {
                    break;
                }

                _visitedHash.Add(pipeline);
                @event(pipeline);
                pipeline = pipeline._parent;
            }

            _visitedHash.Clear();
        }

        private int FindLessOrEqualIndex(int order)
        {
            int left = 0;
            int right = _pipelineStages.Count - 1;

            while (left <= right)
            {
                int mid = left + ((right - left) / 2);

                if (_pipelineStages[mid].Order > order)
                {
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            return left;
        }

        private readonly struct PipelineStageEntry
        {
            public PipelineStageEntry(IPipelineStage stage, int order)
            {
                Stage = stage;
                Order = order;
            }

            public IPipelineStage Stage { get; }

            public int Order { get; }
        }
    }

    public class Scheduler<T> : IDisposable
    {
        private readonly Queue<T> _queue;
        private readonly Action<T> _workAction;

        private bool _disposed;

        public Scheduler(Action<T> workAction)
        {
            ThrowIf.Null(workAction, nameof(workAction));

            _queue = new Queue<T>();
            _workAction = workAction;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            Work();

            _queue.Clear();
        }

        public void Schedule(T item)
        {
            ThrowIf.Null(item, nameof(item));

            _queue.Enqueue(item);
        }

        public void Work()
        {
            while (_queue.Count > 0)
            {
                T item = _queue.Dequeue();

                _workAction(item);
            }
        }
    }
}
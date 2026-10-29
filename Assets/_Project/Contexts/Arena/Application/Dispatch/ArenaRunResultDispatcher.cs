using System;
using System.Collections.Generic;
using Game.Arena.Application.Diagnostics;
using Game.Arena.Application.Hud;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Ticks;

namespace Game.Arena.Application.Dispatch
{
    public sealed class ArenaRunResultDispatcher : IArenaRunDispatch
    {
        private static readonly ArenaRunDispatchStage[] Stages =
        {
            ArenaRunDispatchStage.SnapshotPublication,
            ArenaRunDispatchStage.EntityViewSynchronization,
            ArenaRunDispatchStage.GameplayVisual,
            ArenaRunDispatchStage.Hud,
            ArenaRunDispatchStage.AudioAndVisualEffects,
            ArenaRunDispatchStage.AnalyticsAndDiagnostics,
        };

        // TEMPORARY attribution support (t9-gc-residual-b): a precomputed name
        // table avoids the per-frame string allocation of Enum.ToString().
        private static readonly string[] StageNames =
        {
            nameof(ArenaRunDispatchStage.SnapshotPublication),
            nameof(ArenaRunDispatchStage.EntityViewSynchronization),
            nameof(ArenaRunDispatchStage.GameplayVisual),
            nameof(ArenaRunDispatchStage.Hud),
            nameof(ArenaRunDispatchStage.AudioAndVisualEffects),
            nameof(ArenaRunDispatchStage.AnalyticsAndDiagnostics),
        };

        private readonly Dictionary<ArenaRunDispatchStage, List<SnapshotRegistration>> _snapshotHandlers;
        private readonly Dictionary<ArenaRunDispatchStage, List<HudRegistration>> _hudHandlers;
        private readonly Dictionary<ArenaRunDispatchStage, List<NotificationRegistration>> _notificationHandlers;
        private readonly IArenaRunDispatchDiagnostics _diagnostics;

        private bool _registrationClosed;
        private bool _dispatching;

        public ArenaRunResultDispatcher()
            : this(null)
        {
        }

        public ArenaRunResultDispatcher(IArenaRunDispatchDiagnostics diagnostics)
        {
            _diagnostics = diagnostics;
            _snapshotHandlers = new Dictionary<ArenaRunDispatchStage, List<SnapshotRegistration>>();
            _hudHandlers = new Dictionary<ArenaRunDispatchStage, List<HudRegistration>>();
            _notificationHandlers = new Dictionary<ArenaRunDispatchStage, List<NotificationRegistration>>();
        }

        public void RegisterSnapshotHandler(
            ArenaRunDispatchStage stage,
            IArenaRunSnapshotHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            EnsureRegistrationOpen();

            List<SnapshotRegistration> registrations = GetSnapshotRegistrations(stage);

            for (int index = 0; index < registrations.Count; index++)
            {
                if (ReferenceEquals(registrations[index].Handler, handler))
                {
                    throw new ArgumentException(
                        "The handler is already registered for this stage.",
                        nameof(handler));
                }
            }

            registrations.Add(new SnapshotRegistration(handler));
        }

        public void RegisterHudHandler(
            ArenaRunDispatchStage stage,
            IArenaRunHudHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            EnsureRegistrationOpen();

            List<HudRegistration> registrations = GetHudRegistrations(stage);

            for (int index = 0; index < registrations.Count; index++)
            {
                if (ReferenceEquals(registrations[index].Handler, handler))
                {
                    throw new ArgumentException(
                        "The handler is already registered for this stage.",
                        nameof(handler));
                }
            }

            registrations.Add(new HudRegistration(handler));
        }

        public void RegisterNotificationHandler<TNotification>(
            ArenaRunDispatchStage stage,
            IArenaRunNotificationHandler<TNotification> handler)
            where TNotification : IArenaRunNotification
        {
            if (IsNotificationStage(stage) == false)
            {
                throw new ArgumentException(
                    "The stage does not accept notification handlers.",
                    nameof(stage));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            EnsureRegistrationOpen();

            List<NotificationRegistration> registrations = GetNotificationRegistrations(stage);
            Type notificationType = typeof(TNotification);

            for (int index = 0; index < registrations.Count; index++)
            {
                if (registrations[index].NotificationType == notificationType
                    && ReferenceEquals(registrations[index].Handler, handler))
                {
                    throw new ArgumentException(
                        "The handler is already registered for this stage and notification type.",
                        nameof(handler));
                }
            }

            registrations.Add(new NotificationRegistration(
                notificationType,
                handler,
                notification => handler.Handle((TNotification)notification)));
        }

        public void Dispatch(ArenaRunTickResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.IsPublished)
            {
                ArenaTickLifetimeGuard.Validate(result.Generation);
            }

            if (_dispatching)
            {
                throw new InvalidOperationException(
                    "Dispatch is not re-entrant.");
            }

            _registrationClosed = true;
            _dispatching = true;

            // TEMPORARY attribution switch (t9-gc-residual-b); reverted before
            // the acceptance runs.
            if (AttributionFlags.SkipDispatch)
            {
                _dispatching = false;
                return;
            }

            try
            {
                for (int index = 0; index < Stages.Length; index++)
                {
                    ArenaRunDispatchStage stage = Stages[index];

                    DispatchSnapshotHandlers(stage, result.Snapshot);
                    DispatchHudHandlers(stage, result.Snapshot, result.Hud);
                    DispatchNotificationHandlers(stage, result.Notifications);
                }
            }
            finally
            {
                _dispatching = false;
            }
        }

        private static bool IsNotificationStage(ArenaRunDispatchStage stage)
        {
            return stage == ArenaRunDispatchStage.GameplayVisual
                || stage == ArenaRunDispatchStage.Hud
                || stage == ArenaRunDispatchStage.AudioAndVisualEffects
                || stage == ArenaRunDispatchStage.AnalyticsAndDiagnostics;
        }

        private List<SnapshotRegistration> GetSnapshotRegistrations(ArenaRunDispatchStage stage)
        {
            if (_snapshotHandlers.TryGetValue(stage, out List<SnapshotRegistration> registrations) == false)
            {
                registrations = new List<SnapshotRegistration>();
                _snapshotHandlers.Add(stage, registrations);
            }

            return registrations;
        }

        private List<HudRegistration> GetHudRegistrations(ArenaRunDispatchStage stage)
        {
            if (_hudHandlers.TryGetValue(stage, out List<HudRegistration> registrations) == false)
            {
                registrations = new List<HudRegistration>();
                _hudHandlers.Add(stage, registrations);
            }

            return registrations;
        }

        private List<NotificationRegistration> GetNotificationRegistrations(ArenaRunDispatchStage stage)
        {
            if (_notificationHandlers.TryGetValue(stage, out List<NotificationRegistration> registrations) == false)
            {
                registrations = new List<NotificationRegistration>();
                _notificationHandlers.Add(stage, registrations);
            }

            return registrations;
        }

        private void EnsureRegistrationOpen()
        {
            if (_registrationClosed)
            {
                throw new InvalidOperationException(
                    "Handlers cannot be registered after the first dispatch.");
            }
        }

        private void DispatchSnapshotHandlers(
            ArenaRunDispatchStage stage,
            ArenaRunSnapshot snapshot)
        {
            if (AttributionFlags.SkipsDispatchStage(StageNames[(int)stage]))
            {
                return;
            }

            if (_snapshotHandlers.TryGetValue(stage, out List<SnapshotRegistration> registrations) == false)
            {
                return;
            }

            for (int index = 0; index < registrations.Count; index++)
            {
                SnapshotRegistration registration = registrations[index];

                if (AttributionFlags.SkipsHandler(registration.HandlerName))
                {
                    continue;
                }

                try
                {
                    registration.Handler.Handle(snapshot);
                }
                catch (Exception exception)
                {
                    Report(stage, registration.Handler, typeof(ArenaRunSnapshot), exception);
                }
            }
        }

        private void DispatchHudHandlers(
            ArenaRunDispatchStage stage,
            ArenaRunSnapshot snapshot,
            ArenaRunHudSnapshot hud)
        {
            if (AttributionFlags.SkipsDispatchStage(StageNames[(int)stage]))
            {
                return;
            }

            if (_hudHandlers.TryGetValue(stage, out List<HudRegistration> registrations) == false)
            {
                return;
            }

            for (int index = 0; index < registrations.Count; index++)
            {
                HudRegistration registration = registrations[index];

                if (AttributionFlags.SkipsHandler(registration.HandlerName))
                {
                    continue;
                }

                try
                {
                    registration.Handler.Handle(snapshot, hud);
                }
                catch (Exception exception)
                {
                    Report(stage, registration.Handler, typeof(ArenaRunHudSnapshot), exception);
                }
            }
        }

        private void DispatchNotificationHandlers(
            ArenaRunDispatchStage stage,
            IReadOnlyList<IArenaRunNotification> notifications)
        {
            if (AttributionFlags.SkipsDispatchStage(StageNames[(int)stage]))
            {
                return;
            }

            if (_notificationHandlers.TryGetValue(stage, out List<NotificationRegistration> registrations) == false)
            {
                return;
            }

            for (int notificationIndex = 0; notificationIndex < notifications.Count; notificationIndex++)
            {
                IArenaRunNotification notification = notifications[notificationIndex];

                for (int handlerIndex = 0; handlerIndex < registrations.Count; handlerIndex++)
                {
                    NotificationRegistration registration = registrations[handlerIndex];

                    if (AttributionFlags.SkipsHandler(registration.HandlerName))
                    {
                        continue;
                    }

                    if (registration.NotificationType.IsInstanceOfType(notification) == false)
                    {
                        continue;
                    }

                    try
                    {
                        registration.Invoke(notification);
                    }
                    catch (Exception exception)
                    {
                        Report(stage, registration.Handler, notification.GetType(), exception);
                    }
                }
            }
        }

        private void Report(
            ArenaRunDispatchStage stage,
            object handler,
            Type payloadType,
            Exception exception)
        {
            if (_diagnostics == null)
            {
                return;
            }

            try
            {
                _diagnostics.OnHandlerFailed(new ArenaRunDispatchFailure(
                    stage,
                    handler.GetType().Name,
                    payloadType,
                    exception));
            }
            catch (Exception)
            {
                // A diagnostics failure must not change gameplay behaviour.
            }
        }

        private sealed class SnapshotRegistration
        {
            public SnapshotRegistration(IArenaRunSnapshotHandler handler)
            {
                Handler = handler;
                HandlerName = handler.GetType().Name;
            }

            public IArenaRunSnapshotHandler Handler { get; }

            public string HandlerName { get; }
        }

        private sealed class HudRegistration
        {
            public HudRegistration(IArenaRunHudHandler handler)
            {
                Handler = handler;
                HandlerName = handler.GetType().Name;
            }

            public IArenaRunHudHandler Handler { get; }

            public string HandlerName { get; }
        }

        private sealed class NotificationRegistration
        {
            public NotificationRegistration(
                Type notificationType,
                object handler,
                Action<IArenaRunNotification> invoke)
            {
                NotificationType = notificationType;
                Handler = handler;
                HandlerName = handler.GetType().Name;
                Invoke = invoke;
            }

            public Type NotificationType { get; }

            public object Handler { get; }

            public string HandlerName { get; }

            public Action<IArenaRunNotification> Invoke { get; }
        }
    }
}

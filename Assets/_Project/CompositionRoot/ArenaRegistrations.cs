using System;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Identity;
using Game.Arena.Application.Ports;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Restart;
using Game.Arena.Application.Sessions;
using Game.Arena.Application.Spawning;
using Game.Arena.Application.Ticks;
using Game.Arena.Application.Time;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Movement;
using Game.Arena.Domain.Repositories;
using Game.Arena.Infrastructure.Movement;
using Game.Arena.Infrastructure.Repositories;
using Game.Arena.Infrastructure.Scenes;
using Game.Arena.Infrastructure.Spawning;
using Game.Arena.Infrastructure.Targeting;
using Game.Arena.Infrastructure.Time;
using VContainer;
using VContainer.Unity;

namespace Game.CompositionRoot
{
    /// <summary>
    /// The single catalogue of Arena registrations: process-lifetime services in
    /// the persistent root scope, scene services in the per-scene child scope.
    /// </summary>
    public static class ArenaRegistrations
    {
        public static void RegisterProcessLifetime(
            IContainerBuilder builder,
            ArenaGameConfiguration configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            builder.RegisterInstance(configuration);
            builder.RegisterInstance(configuration.CreationParameters);

            builder.Register<IArenaRunRepository, InMemoryArenaRunRepository>(Lifetime.Singleton);
            builder.Register<IArenaRunIdSource>(
                _ => new MonotonicArenaRunIdSource(),
                Lifetime.Singleton);
            builder.Register<IPlayerIdSource>(
                _ => new MonotonicPlayerIdSource(),
                Lifetime.Singleton);
            builder.Register<IEnemyIdSource>(
                _ => new MonotonicEnemyIdSource(),
                Lifetime.Singleton);

            builder.Register<MovementPathPolicy>(Lifetime.Singleton);
            builder.Register<ArenaRunFactory>(Lifetime.Singleton);
            builder.Register<ArenaRunSnapshotMapper>(Lifetime.Singleton);
            builder.Register<ArenaRunSessionDependencies>(Lifetime.Singleton);
            builder.Register<ArenaRunSession>(Lifetime.Singleton);

            builder.Register<IRunRestartDelayPolicy>(
                _ => new FixedIntervalRunRestartDelayPolicy(configuration.RestartDelay),
                Lifetime.Singleton);
        }

        public static void RegisterSceneLifetime(
            IContainerBuilder builder,
            ArenaSceneObjects sceneObjects)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (sceneObjects == null)
            {
                throw new ArgumentNullException(nameof(sceneObjects));
            }

            builder.RegisterInstance(sceneObjects);
            builder.RegisterInstance(sceneObjects.EnemyTransforms);

            builder.Register<IFrameTimeSource, UnityFrameTimeSource>(Lifetime.Singleton);
            builder.Register<IGameClock, FrameDeltaGameClock>(Lifetime.Singleton);
            builder.RegisterInstance(sceneObjects.InputSource);

            EnemyColliderRegistry colliders = new();
            builder.RegisterInstance<IEnemyColliderRegistry, IEnemyColliderRegistration>(colliders);

            builder.Register<IPlayerMovementResolver>(
                _ => new UnityPlayerMovementResolver(sceneObjects.Player),
                Lifetime.Singleton);
            builder.Register<IPlayerAttackTargetingResolver>(
                _ => new UnityPlayerAttackTargetingResolver(colliders),
                Lifetime.Singleton);
            builder.Register<IEnemyMovementResolver>(
                container => new UnityEnemyMovementResolver(
                    sceneObjects.EnemyTransforms,
                    container.Resolve<ArenaGameConfiguration>().Arena),
                Lifetime.Singleton);
            builder.Register<IEnemySpawnPlacementResolver>(
                container => new UnityEnemySpawnPlacementResolver(
                    container.Resolve<ArenaGameConfiguration>().Arena,
                    sceneObjects.Camera,
                    sceneObjects.Player),
                Lifetime.Singleton);
            builder.Register<IArenaSceneReloader>(
                container => new UnityArenaSceneReloader(
                    container.Resolve<ArenaGameConfiguration>().ArenaSceneName),
                Lifetime.Singleton);
            builder.Register<IEnemySpawnPacingPolicy>(
                container => new FixedIntervalSpawnPacingPolicy(
                    container.Resolve<ArenaGameConfiguration>().SpawnInterval),
                Lifetime.Singleton);

            builder.Register<IArenaRunDispatchDiagnostics, NoOpArenaRunDispatchDiagnostics>(
                Lifetime.Singleton);

            builder.Register<IArenaRunDispatch, ArenaRunResultDispatcher>(Lifetime.Singleton)
                .AsSelf();

            builder.Register<ArenaRunTickServiceDependencies>(Lifetime.Singleton);
            builder.Register<ArenaRunTickService>(Lifetime.Singleton);

            builder.RegisterEntryPoint<ArenaTickDriver>(Lifetime.Singleton);
        }
    }
}

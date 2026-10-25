using System;
using System.Collections.Generic;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Identity;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
using Game.Arena.Application.Restart;
using Game.Arena.Application.Sessions;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Repositories;
using Game.Arena.Infrastructure.Movement;
using NUnit.Framework;
using UnityEngine;
using VContainer;

namespace Game.CompositionRoot.PlayMode.Tests
{
    /// <summary>
    /// Proves the composition catalogue (ADR-0030) builds both graphs: the
    /// process-lifetime root and the scene-lifetime child over an explicit
    /// <see cref="ArenaSceneObjects"/> seam.
    /// </summary>
    [TestFixture]
    public sealed class ArenaCompositionTests
    {
        private readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _objects.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
        }

        [Test]
        public void ProcessLifetimeGraph_ResolvesEveryService()
        {
            ContainerBuilder builder = new();
            ArenaGameConfiguration configuration = ArenaGameConfiguration.CreateDefault();
            ArenaRegistrations.RegisterProcessLifetime(builder, configuration);

            using IObjectResolver container = builder.Build();

            Assert.AreSame(configuration, container.Resolve<ArenaGameConfiguration>());
            Assert.IsNotNull(container.Resolve<ArenaRunCreationParameters>());
            Assert.IsNotNull(container.Resolve<IArenaRunRepository>());
            Assert.IsNotNull(container.Resolve<IArenaRunIdSource>());
            Assert.IsNotNull(container.Resolve<IPlayerIdSource>());
            Assert.IsNotNull(container.Resolve<IEnemyIdSource>());
            Assert.IsNotNull(container.Resolve<ArenaRunSession>());
            Assert.IsNotNull(container.Resolve<IRunRestartDelayPolicy>());
        }

        [Test]
        public void SceneLifetimeGraph_ResolvesTickServiceAndDispatcher()
        {
            ContainerBuilder builder = new();
            ArenaGameConfiguration configuration = ArenaGameConfiguration.CreateDefault();
            ArenaRegistrations.RegisterProcessLifetime(builder, configuration);
            ArenaRegistrations.RegisterSceneLifetime(builder, CreateSceneObjects(configuration));

            using IObjectResolver container = builder.Build();

            ArenaRunTickServiceDependencies dependencies =
                container.Resolve<ArenaRunTickServiceDependencies>();

            Assert.IsNotNull(container.Resolve<ArenaRunTickService>());
            Assert.IsNotNull(container.Resolve<IArenaRunDispatch>());
            Assert.IsNotNull(dependencies.Session);
            Assert.IsNotNull(dependencies.Clock);
            Assert.IsNotNull(dependencies.InputSource);
            Assert.IsNotNull(dependencies.MovementResolver);
            Assert.IsNotNull(dependencies.TargetingResolver);
            Assert.IsNotNull(dependencies.EnemyMovementResolver);
            Assert.IsNotNull(dependencies.SpawnPlacementResolver);
            Assert.IsNotNull(dependencies.EnemyIdSource);
            Assert.IsNotNull(dependencies.PacingPolicy);
            Assert.IsNotNull(dependencies.RestartDelayPolicy);
            Assert.IsNotNull(dependencies.SceneReloader);
            Assert.IsNotNull(dependencies.Dispatcher);
        }

        [Test]
        public void SingletonRegistrations_ReturnTheSameInstance()
        {
            ContainerBuilder builder = new();
            ArenaGameConfiguration configuration = ArenaGameConfiguration.CreateDefault();
            ArenaRegistrations.RegisterProcessLifetime(builder, configuration);
            ArenaRegistrations.RegisterSceneLifetime(builder, CreateSceneObjects(configuration));

            using IObjectResolver container = builder.Build();

            Assert.AreSame(
                container.Resolve<ArenaRunSession>(),
                container.Resolve<ArenaRunSession>());
            Assert.AreSame(
                container.Resolve<IArenaRunDispatch>(),
                container.Resolve<IArenaRunDispatch>());
        }

        [Test]
        public void SceneScope_MissingSeam_FailsLoudly()
        {
            ContainerBuilder builder = new();

            Assert.Throws<ArgumentNullException>(
                () => ArenaRegistrations.RegisterSceneLifetime(builder, null));

            GameObject scopeObject = new("Arena Scene Scope");
            _objects.Add(scopeObject);
            ArenaSceneLifetimeScope scope = scopeObject.AddComponent<ArenaSceneLifetimeScope>();

            Assert.Throws<InvalidOperationException>(() => scope.Build());
        }

        private ArenaSceneObjects CreateSceneObjects(ArenaGameConfiguration configuration)
        {
            GameObject cameraObject = new("Composition Test Camera");
            _objects.Add(cameraObject);
            Camera camera = cameraObject.AddComponent<Camera>();

            GameObject playerObject = new("Composition Test Player");
            _objects.Add(playerObject);

            return new ArenaSceneObjects(
                configuration,
                camera,
                playerObject.transform,
                new FakePlayerInputSource(),
                new FakeEnemyTransformRegistry());
        }

        private sealed class FakePlayerInputSource : IPlayerInputSource
        {
            public PlayerFrameInput Read()
            {
                return new PlayerFrameInput(
                    MovementAxes.Zero,
                    Direction3D.Forward,
                    attackPressed: false,
                    switchWeaponPressed: false);
            }
        }

        private sealed class FakeEnemyTransformRegistry : IEnemyTransformRegistry
        {
            public bool TryGetTransform(EnemyId enemyId, out Transform transform)
            {
                transform = null;
                return false;
            }
        }
    }
}

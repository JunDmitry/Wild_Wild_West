using System;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Identity;
using Game.Arena.Application.Ports;
using Game.Arena.Application.Restart;
using Game.Arena.Application.Sessions;
using Game.Arena.Application.Spawning;

namespace Game.Arena.Application.Ticks
{
    public sealed class ArenaRunTickServiceDependencies
    {
        public ArenaRunTickServiceDependencies(
            ArenaRunSession session,
            IGameClock clock,
            IPlayerInputSource inputSource,
            IPlayerMovementResolver movementResolver,
            IPlayerAttackTargetingResolver targetingResolver,
            IEnemyMovementResolver enemyMovementResolver,
            IEnemySpawnPlacementResolver spawnPlacementResolver,
            IEnemyIdSource enemyIdSource,
            IEnemySpawnPacingPolicy pacingPolicy,
            IRunRestartDelayPolicy restartDelayPolicy,
            IArenaSceneReloader sceneReloader,
            IArenaRunDispatch dispatcher)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            InputSource = inputSource ?? throw new ArgumentNullException(nameof(inputSource));
            MovementResolver = movementResolver ?? throw new ArgumentNullException(nameof(movementResolver));
            TargetingResolver = targetingResolver ?? throw new ArgumentNullException(nameof(targetingResolver));
            EnemyMovementResolver = enemyMovementResolver ?? throw new ArgumentNullException(nameof(enemyMovementResolver));
            SpawnPlacementResolver = spawnPlacementResolver ?? throw new ArgumentNullException(nameof(spawnPlacementResolver));
            EnemyIdSource = enemyIdSource ?? throw new ArgumentNullException(nameof(enemyIdSource));
            PacingPolicy = pacingPolicy ?? throw new ArgumentNullException(nameof(pacingPolicy));
            RestartDelayPolicy = restartDelayPolicy ?? throw new ArgumentNullException(nameof(restartDelayPolicy));
            SceneReloader = sceneReloader ?? throw new ArgumentNullException(nameof(sceneReloader));
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public ArenaRunSession Session { get; }

        public IGameClock Clock { get; }

        public IPlayerInputSource InputSource { get; }

        public IPlayerMovementResolver MovementResolver { get; }

        public IPlayerAttackTargetingResolver TargetingResolver { get; }

        public IEnemyMovementResolver EnemyMovementResolver { get; }

        public IEnemySpawnPlacementResolver SpawnPlacementResolver { get; }

        public IEnemyIdSource EnemyIdSource { get; }

        public IEnemySpawnPacingPolicy PacingPolicy { get; }

        public IRunRestartDelayPolicy RestartDelayPolicy { get; }

        public IArenaSceneReloader SceneReloader { get; }

        public IArenaRunDispatch Dispatcher { get; }
    }
}

using System;
using System.Collections.Generic;
using Game.Arena.Application.Identity;
using Game.Arena.Application.Input;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.Ports;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Restart;
using Game.Arena.Application.Sessions;
using Game.Arena.Application.Spawning;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using Game.Arena.Infrastructure.Repositories;
using Game.Arena.Infrastructure.Targeting;

namespace Game.Arena.PlayMode.Tests
{
    internal sealed class PlayModeTickHarness
    {
        public const int RangedDamage = 10;
        public const int EnemyHealth = 50;

        private static readonly GameDuration TickInterval = new(0.1d);

        private readonly InMemoryArenaRunRepository _repository;
        private readonly ArenaRunSession _session;
        private readonly IPlayerInputSource _input;
        private readonly MutableInputSource _mutableInput;
        private readonly MutableClock _clock;
        private readonly ArenaRunTickService _tickService;
        private readonly ArenaRun _run;

        public PlayModeTickHarness(
            IEnemyColliderRegistry enemyColliders,
            float meleeRange = 2f,
            AttackArc? meleeArc = null,
            IPlayerInputSource playerInputSource = null,
            IPlayerMovementResolver playerMovementResolver = null)
        {
            _repository = new InMemoryArenaRunRepository();
            _mutableInput = playerInputSource == null ? new MutableInputSource(CreateInput(false)) : null;
            _input = playerInputSource ?? _mutableInput;
            _clock = new MutableClock(TickInterval);

            ArenaRunSessionDependencies sessionDependencies = new(
                _repository,
                new MonotonicArenaRunIdSource(),
                new MonotonicPlayerIdSource(),
                new ArenaRunFactory(new MovementPathPolicy()),
                new ArenaRunSnapshotMapper());

            _session = new ArenaRunSession(sessionDependencies, CreateCreationParameters(meleeRange, meleeArc));
            _session.StartInitialRun();

            if (_repository.TryGet(_session.ActiveArenaRunId, out _run) == false)
            {
                throw new InvalidOperationException("Active ArenaRun was not stored.");
            }

            ArenaRunTickServiceDependencies tickDependencies = new(
                _session,
                _clock,
                _input,
                playerMovementResolver ?? new StationaryPlayerMovementResolver(),
                new UnityPlayerAttackTargetingResolver(enemyColliders),
                new NoEnemyMovementResolver(),
                new FixedSpawnPlacementResolver(),
                new MonotonicEnemyIdSource(),
                new NeverSpawnPacingPolicy(),
                new FixedIntervalRunRestartDelayPolicy(new GameDuration(1000d)),
                new NoOpNotificationSink());

            _tickService = new ArenaRunTickService(tickDependencies);
        }

        public EnemyId SeedEnemy(ulong value, Position3D position)
        {
            EnemySpawnRequestOutcome outcome = _run.RequestEnemySpawn();

            if (outcome.HasRequest == false)
            {
                throw new InvalidOperationException("No enemy spawn is available.");
            }

            EnemyId enemyId = EnemyId.FromValue(value);

            EnemySpawnResolutionOutcome resolutionOutcome = _run.ApplyEnemySpawn(
                new EnemySpawnResolution(outcome.Request.Correlation, enemyId, position));

            if (resolutionOutcome.IsSpawned == false)
            {
                throw new InvalidOperationException("Enemy spawn was rejected: " + resolutionOutcome.RejectionReason);
            }

            return enemyId;
        }

        public ArenaRunTickResult Tick(
            bool attackPressed,
            Direction3D viewDirection,
            bool switchWeaponPressed = false)
        {
            if (_mutableInput == null)
            {
                throw new InvalidOperationException(
                    "Tick(input) requires the harness's default mutable input source; call TickFrame() instead.");
            }

            _mutableInput.Input = new PlayerFrameInput(
                MovementAxes.Zero,
                viewDirection,
                attackPressed,
                switchWeaponPressed);

            return _tickService.ExecuteTick();
        }

        public ArenaRunTickResult TickFrame()
        {
            return _tickService.ExecuteTick();
        }

        public ArenaRunTickResult TickIdle()
        {
            return Tick(false, Direction3D.Forward);
        }

        public int ReadEnemyHealth(EnemyId enemyId)
        {
            ArenaRunSnapshot snapshot = new ArenaRunSnapshotMapper().Map(_run.CreateSnapshot());

            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                if (snapshot.Enemies[index].EnemyId == enemyId)
                {
                    return snapshot.Enemies[index].Health.Current;
                }
            }

            throw new InvalidOperationException("Enemy is not present in the snapshot: " + enemyId);
        }

        private static PlayerFrameInput CreateInput(bool attackPressed)
        {
            return new PlayerFrameInput(MovementAxes.Zero, Direction3D.Forward, attackPressed, false);
        }

        private static ArenaRunCreationParameters CreateCreationParameters(
            float meleeRange,
            AttackArc? meleeArc)
        {
            ArenaDefinition arena = new(
                new ArenaBounds(-20f, 20f, -20f, 20f, 0f),
                Distance.FromValue(5f));

            PlayerDefinition player = new(
                Position3D.Zero,
                Health.Full(100),
                MovementSpeed.FromUnitsPerSecond(5f),
                CollisionRadius.FromValue(0.5f));

            WeaponCatalog weapons = new(
                new WeaponDefinition(
                    WeaponKind.Ranged,
                    DamageAmount.FromPoints(RangedDamage),
                    Distance.FromValue(50f),
                    new GameDuration(0.4d),
                    new GameDuration(0d)),
                new WeaponDefinition(
                    WeaponKind.Melee,
                    DamageAmount.FromPoints(20),
                    Distance.FromValue(meleeRange),
                    new GameDuration(0.8d),
                    new GameDuration(0.3d),
                    meleeArc ?? AttackArc.Full));

            EnemyCatalog enemies = new(
                new EnemyDefinition(
                    EnemyKind.Regular,
                    Health.Full(EnemyHealth),
                    MovementSpeed.FromUnitsPerSecond(2f),
                    CollisionRadius.FromValue(0.5f),
                    DamageAmount.FromPoints(5),
                    Distance.FromValue(2f),
                    new GameDuration(1d),
                    new GameDuration(0.2d)),
                new EnemyDefinition(
                    EnemyKind.Boss,
                    Health.Full(300),
                    MovementSpeed.FromUnitsPerSecond(1.75f),
                    CollisionRadius.FromValue(1f),
                    DamageAmount.FromPoints(25),
                    Distance.FromValue(3f),
                    new GameDuration(1.1d),
                    new GameDuration(0.4d)));

            WaveCatalog waves = new(new[] { new WaveDefinition(WaveNumber.First, 10) });

            return new ArenaRunCreationParameters(arena, player, weapons, enemies, waves);
        }

        private sealed class MutableClock : IGameClock
        {
            public MutableClock(GameDuration delta)
            {
                Delta = delta;
            }

            public GameDuration Delta { get; }

            public GameDuration GetDelta()
            {
                return Delta;
            }
        }

        private sealed class MutableInputSource : IPlayerInputSource
        {
            public MutableInputSource(PlayerFrameInput input)
            {
                Input = input;
            }

            public PlayerFrameInput Input { get; set; }

            public PlayerFrameInput Read()
            {
                return Input;
            }
        }

        private sealed class StationaryPlayerMovementResolver : IPlayerMovementResolver
        {
            public Position3D Resolve(PlayerMovementRequest request)
            {
                return request.From;
            }
        }

        private sealed class NoEnemyMovementResolver : IEnemyMovementResolver
        {
            public IReadOnlyList<EnemyMovementBatchResolutionEntry> Resolve(EnemyMovementBatchRequest request)
            {
                return Array.Empty<EnemyMovementBatchResolutionEntry>();
            }
        }

        private sealed class FixedSpawnPlacementResolver : IEnemySpawnPlacementResolver
        {
            public Position3D ResolveSpawnPosition(EnemySpawnRequest request)
            {
                return Position3D.Zero;
            }
        }

        private sealed class NeverSpawnPacingPolicy : IEnemySpawnPacingPolicy
        {
            public bool ShouldRequestSpawn(GameTimePoint now)
            {
                return false;
            }

            public void RecordSuccessfulSpawn(GameTimePoint now)
            {
            }

            public void Reset()
            {
            }
        }

        private sealed class NoOpNotificationSink : IArenaRunNotificationSink
        {
            public void Publish(IArenaRunNotification notification)
            {
            }
        }
    }
}

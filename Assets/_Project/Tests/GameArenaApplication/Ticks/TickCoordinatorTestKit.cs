using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Sessions;
using Game.Arena.Application.Spawning;
using Game.Arena.Application.Tests.Sessions;
using Game.Arena.Application.Ticks;
using Game.Arena.Application.Ticks.Stages;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Spawn;

namespace Game.Arena.Application.Tests.Ticks
{
    internal sealed class TickCoordinatorTestKit
    {
        public TickCoordinatorTestKit()
        {
            Repository = new TestArenaRunRepository();
            ArenaRunIds = new RecordingArenaRunIdSource();
            PlayerIds = new RecordingPlayerIdSource();
            EnemyIds = new RecordingEnemyIdSource();
            CreationParameters = new ArenaRunCreationParametersFactory().Create();
            MovementResolver = new ScriptedPlayerMovementResolver();
            TargetingResolver = new ScriptedTargetingResolver();
            EnemyMovementResolver = new ScriptedEnemyMovementResolver();
            SpawnResolver = new ScriptedSpawnPlacementResolver();
            Pacing = new FixedIntervalSpawnPacingPolicy(new Domain.Time.GameDuration(1d));
            Clock = new FixedGameClock(new Domain.Time.GameDuration(0.1d));
            Input = new FixedPlayerInputSource();
            Tracker = new PendingInteractionTracker();

            ArenaRunSessionDependencies sessionDependencies = new(
                    Repository,
                    ArenaRunIds,
                    PlayerIds,
                    new ArenaRunFactory(
                        new Domain.Movement.MovementPathPolicy()));

            Session = new ArenaRunSession(
                sessionDependencies,
                CreationParameters);

            PlayerPhase = new PlayerTickPhase(
                MovementResolver,
                TargetingResolver,
                Tracker);

            EnemyPhase = new EnemyTickPhase(
                EnemyMovementResolver,
                SpawnResolver,
                EnemyIds,
                Pacing,
                Tracker);

            RecoveryStage = new PendingInteractionRecoveryStage(Tracker);
            TimeAdvanceStage = new TimeAdvanceStage();
            SnapshotMapper = new();

            ArenaRunTickCoordinatorDependencies dependencies = new(
                    Session,
                    Clock,
                    Input,
                    PlayerPhase,
                    EnemyPhase,
                    RecoveryStage,
                    TimeAdvanceStage,
                    SnapshotMapper);

            Coordinator = new ArenaRunTickCoordinator(dependencies);
        }

        public TestArenaRunRepository Repository { get; }

        public RecordingArenaRunIdSource ArenaRunIds { get; }

        public RecordingPlayerIdSource PlayerIds { get; }

        public RecordingEnemyIdSource EnemyIds { get; }

        public ArenaRunCreationParameters CreationParameters { get; }

        public ScriptedPlayerMovementResolver MovementResolver { get; }

        public ScriptedTargetingResolver TargetingResolver { get; }

        public ScriptedEnemyMovementResolver EnemyMovementResolver { get; }

        public ScriptedSpawnPlacementResolver SpawnResolver { get; }

        public FixedIntervalSpawnPacingPolicy Pacing { get; }

        public FixedGameClock Clock { get; }

        public FixedPlayerInputSource Input { get; }

        public PendingInteractionTracker Tracker { get; }

        public ArenaRunSession Session { get; }

        public PlayerTickPhase PlayerPhase { get; }

        public EnemyTickPhase EnemyPhase { get; }

        public PendingInteractionRecoveryStage RecoveryStage { get; }

        public TimeAdvanceStage TimeAdvanceStage { get; }

        public ArenaRunTickCoordinator Coordinator { get; }

        public ArenaRunSnapshotMapper SnapshotMapper { get; }

        public EnemyId Spawn(ArenaRun run, ulong value, Position3D position)
        {
            EnemySpawnRequestOutcome requestOutcome = run.RequestEnemySpawn();

            if (requestOutcome.HasRequest == false)
            {
                throw new System.Exception("No available spawns.");
            }

            EnemySpawnRequest request = requestOutcome.Request;
            EnemyId enemyId = EnemyId.FromValue(value);

            run.ApplyEnemySpawn(new EnemySpawnResolution(request.Correlation, enemyId, position));

            return enemyId;
        }
    }
}

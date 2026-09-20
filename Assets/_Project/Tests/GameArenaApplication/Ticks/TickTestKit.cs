using Game.Arena.Application.Input;
using Game.Arena.Application.Sessions;
using Game.Arena.Application.Spawning;
using Game.Arena.Application.Tests.Sessions;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Application.Tests.Ticks
{
    internal sealed class TickTestKit
    {
        private readonly ArenaRunFactory _factory;
        private readonly ArenaRunCreationParameters _standardParameters;

        public TickTestKit()
        {
            _factory = new ArenaRunFactory(new MovementPathPolicy());
            _standardParameters = new ArenaRunCreationParametersFactory().Create();
        }

        public ArenaRun StartStandartRun()
        {
            return Start(_standardParameters, 100);
        }

        public ArenaRun StartCompactRun(int playerHealth)
        {
            ArenaRunCreationParameters compact = new(
                new ArenaDefinition(
                    new ArenaBounds(-1f, 1f, -1f, 1f, 0f),
                    Distance.FromValue(5f)),
                _standardParameters.PlayerDefinition,
                _standardParameters.WeaponCatalog,
                _standardParameters.EnemyCatalog,
                _standardParameters.WaveCatalog);

            return Start(compact, playerHealth);
        }

        public PlayerFrameInput Input(
            Displacement3D movement,
            Direction3D aim,
            bool attack,
            bool switchWeapon)
        {
            return new PlayerFrameInput(
                MovementInput.FromVector(movement),
                aim,
                attack,
                switchWeapon);
        }

        public PlayerFrameInput Idle()
        {
            return Input(Displacement3D.Zero, Direction3D.Right, false, false);
        }

        public EnemyId Spawn(ArenaRun run, ulong value, Position3D position)
        {
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;
            EnemyId enemyId = EnemyId.FromValue(value);

            run.ApplyEnemySpawn(new EnemySpawnResolution(request.Correlation, enemyId, position));

            return enemyId;
        }

        public void DefeatPlayer(ArenaRun run)
        {
            Spawn(run, 900UL, new Position3D(2f, 0f, 0f));
            run.StartEligibleEnemyAttacks();
            run.AdvanceTime(new GameDuration(0.2d));
            run.ResolveDueEnemyAttackImpacts();
        }

        public ArenaRun StartCompactRun(int playerHealth, int regularEnemiesInFirstWave)
        {
            ArenaRunCreationParameters compact = new(
                new ArenaDefinition(
                    new ArenaBounds(-1f, 1f, -1f, 1f, 0f),
                    Distance.FromValue(5f)),
                _standardParameters.PlayerDefinition,
                _standardParameters.WeaponCatalog,
                _standardParameters.EnemyCatalog,
                new WaveCatalog(
                    new[]
                    {
                        new WaveDefinition(WaveNumber.First, regularEnemiesInFirstWave),
                        new WaveDefinition(WaveNumber.First.Next(), 1),
                    }));

            return Start(compact, playerHealth);
        }

        private ArenaRun Start(ArenaRunCreationParameters parameters, int playerHealth)
        {
            PlayerDefinition player = new(
                parameters.PlayerDefinition.StartPosition,
                Health.Full(playerHealth),
                parameters.PlayerDefinition.MovementSpeed,
                parameters.PlayerDefinition.CollisionRadius);

            return _factory.Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(1UL),
                parameters.ArenaDefinition,
                player,
                parameters.EnemyCatalog,
                parameters.WeaponCatalog,
                parameters.WaveCatalog);
        }
    }
}

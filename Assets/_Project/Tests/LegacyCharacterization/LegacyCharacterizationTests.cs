using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Initializers;
using Game.Core.Model.Results;
using Game.Core.Model.Simulation.Queries;
using Game.Core.Model.Simulation.Resolutions;
using Game.Core.Model.States;
using Game.Core.Rules;
using NUnit.Framework;

namespace Game.Legacy.Characterization.Tests
{
    [TestFixture]
    [Category("LegacyCharacterization")]
    public sealed class LegacyCharacterizationTests
    {
        private GameConfig _config;
        private LegacyScenarioFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new LegacyScenarioFactory();
            _config = _factory.CreateConfig();
        }

        [Test]
        public void NewGameStartsWithFirstRegularCombatWave()
        {
            EntityId playerId = new(1);

            GameState state = GameInit.NewGame(
                _config,
                playerId,
                1);

            Assert.That(state.Phase, Is.EqualTo(GamePhase.Playing));
            Assert.That(state.Player.Id, Is.EqualTo(playerId));
            Assert.That(state.Player.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
            Assert.That(state.Player.CurrentHealth, Is.EqualTo(state.Player.MaxHealth));
            Assert.That(state.CurrentWave.Number, Is.EqualTo(1));
            Assert.That(state.CurrentWave.Phase, Is.EqualTo(WavePhase.RegularCombat));
            Assert.That(state.Enemies, Is.Empty);
        }

        [Test]
        public void UnblockedMovementChangesPlayerPosition()
        {
            GameState state = GameInit.NewGame(
                _config,
                new EntityId(1),
                1);

            FrameInput input = new(
                Direction3D.Right,
                Position3D.Zero,
                Direction3D.Forward,
                false,
                false);

            SimulationPlan plan = Simulation.Plan(
                state,
                _config,
                input,
                0.1f);

            ResolvedSimulationContext context = new(
                    MovementResolution.Unblocked,
                    AttackResolution.Empty,
                    SpawnResolution.None);

            SimulationOutput output = Simulation.Execute(
                state,
                _config,
                input,
                plan,
                context,
                0.1f);

            Assert.That(output.State.Player.Position.X, Is.EqualTo(0.5f));
            Assert.That(output.State.Player.Position.Y, Is.EqualTo(0f));
            Assert.That(output.State.Player.Position.Z, Is.EqualTo(0f));
        }

        [Test]
        public void BlockedMovementPreservesPlayerPosition()
        {
            GameState state = GameInit.NewGame(
                _config,
                new EntityId(1),
                1);

            FrameInput input = new(
                Direction3D.Right,
                Position3D.Zero,
                Direction3D.Forward,
                false,
                false);

            SimulationPlan plan = Simulation.Plan(
                state,
                _config,
                input,
                0.1f);

            ResolvedSimulationContext context = new(
                    MovementResolution.Blocked,
                    AttackResolution.Empty,
                    SpawnResolution.None);

            SimulationOutput output = Simulation.Execute(
                state,
                _config,
                input,
                plan,
                context,
                0.1f);

            Assert.That(output.State.Player.Position, Is.EqualTo(state.Player.Position));
        }

        [Test]
        public void RangedAttackDamagesResolvedEnemy()
        {
            EntityId enemyId = new EntityId(2);
            PlayerState player = _factory.CreatePlayer(
                new EntityId(1),
                WeaponKind.Ranged);

            EnemyState enemy = _factory.CreateEnemy(
                enemyId,
                EnemyKind.Regular,
                new Position3D(10f, 0f, 0f),
                50f);

            WaveState wave = new(
                1,
                WavePhase.RegularCombat,
                0,
                1,
                BossStatus.NotSpawned);

            GameState state = _factory.CreateState(
                player,
                _factory.CreateEnemies(enemy),
                wave);

            FrameInput input = new(
                Direction3D.Zero,
                Position3D.Zero,
                Direction3D.Right,
                true,
                false);

            SimulationPlan plan = Simulation.Plan(
                state,
                _config,
                input,
                0.1f);

            ResolvedSimulationContext context = new(
                    MovementResolution.Unblocked,
                    AttackResolution.RangedHit(enemyId),
                    SpawnResolution.None);

            SimulationOutput output = Simulation.Execute(
                state,
                _config,
                input,
                plan,
                context,
                0.1f);

            Assert.That(output.State.Enemies[enemyId].CurrentHealth, Is.EqualTo(40f));
            Assert.That(output.Result.PlayerAttacked, Is.True);
            Assert.That(output.Result.PlayerAttackHit, Is.True);
            Assert.That(output.Result.EnemiesDamaged.Count, Is.EqualTo(1));
        }

        [Test]
        public void SuccessfulSpawnUsesExternallyAllocatedIdentifier()
        {
            GameState state = GameInit.NewGame(
                _config,
                new EntityId(1),
                1);

            FrameInput input = new(
                Direction3D.Zero,
                Position3D.Zero,
                Direction3D.Forward,
                false,
                false);

            SimulationPlan plan = Simulation.Plan(
                state,
                _config,
                input,
                0.1f);

            EntityId enemyId = new(42);
            Position3D spawnPosition = new(25f, 0f, 0f);

            ResolvedSimulationContext context =
                new ResolvedSimulationContext(
                    MovementResolution.Unblocked,
                    AttackResolution.Empty,
                    SpawnResolution.Successful(enemyId, spawnPosition));

            SimulationOutput output = Simulation.Execute(
                state,
                _config,
                input,
                plan,
                context,
                0.1f);

            Assert.That(output.State.Enemies.ContainsKey(enemyId), Is.True);
            Assert.That(
                output.State.Enemies[enemyId].Position,
                Is.EqualTo(spawnPosition));
            Assert.That(output.Result.EnemiesSpawned.Count, Is.EqualTo(1));
            Assert.That(
                output.Result.EnemiesSpawned[0].Id,
                Is.EqualTo(enemyId));
        }

        [Test]
        public void PlayerDefeatChangesGamePhase()
        {
            EntityId enemyId = new(2);

            PlayerState player = _factory.CreatePlayer(
                new EntityId(1),
                WeaponKind.Ranged,
                5f);

            EnemyState enemy = _factory.CreateEnemy(
                enemyId,
                EnemyKind.Regular,
                new Position3D(1f, 0f, 0f),
                50f);

            WaveState wave = new(
                1,
                WavePhase.RegularCombat,
                0,
                1,
                BossStatus.NotSpawned);

            GameState state = _factory.CreateState(
                player,
                _factory.CreateEnemies(enemy),
                wave);

            FrameInput input = new(
                Direction3D.Zero,
                Position3D.Zero,
                Direction3D.Forward,
                false,
                false);

            SimulationPlan plan = Simulation.Plan(
                state,
                _config,
                input,
                0.1f);

            SimulationOutput output = Simulation.Execute(
                state,
                _config,
                input,
                plan,
                ResolvedSimulationContext.Empty,
                0.1f);

            Assert.That(output.State.Phase, Is.EqualTo(GamePhase.Defeat));
            Assert.That(output.State.Player.CurrentHealth, Is.EqualTo(0f));
            Assert.That(output.State.PhaseEnteredTime, Is.EqualTo(0.1f));
            Assert.That(output.Result.PlayerDefeated, Is.True);
            Assert.That(output.Result.GamePhaseChanged, Is.True);
        }

        [Test]
        public void DefeatRequestsRestartAfterConfiguredDelay()
        {
            PlayerState player = _factory.CreatePlayer(
                new EntityId(1),
                WeaponKind.Ranged,
                0f);

            WaveState wave = new(
                1,
                WavePhase.RegularCombat,
                0,
                0,
                BossStatus.NotSpawned);

            GameState state = _factory.CreateState(
                player,
                new Dictionary<EntityId, EnemyState>(),
                wave,
                GamePhase.Defeat,
                10f,
                10f);

            FrameInput input = new(
                Direction3D.Zero,
                Position3D.Zero,
                Direction3D.Forward,
                false,
                false);

            SimulationPlan plan = Simulation.Plan(
                state,
                _config,
                input,
                2f);

            SimulationOutput output = Simulation.Execute(
                state,
                _config,
                input,
                plan,
                ResolvedSimulationContext.Empty,
                2f);

            Assert.That(output.State.Phase, Is.EqualTo(GamePhase.Defeat));
            Assert.That(output.Result.RestartRequested, Is.True);
        }

        [Test]
        public void DefeatedFinalBossChangesGamePhaseToVictory()
        {
            EntityId bossId = new(2);

            PlayerState player = _factory.CreatePlayer(
                new EntityId(1),
                WeaponKind.Ranged);

            EnemyState boss = _factory.CreateEnemy(
                bossId,
                EnemyKind.Boss,
                new Position3D(10f, 0f, 0f),
                10f);

            WaveState wave = new(
                3,
                WavePhase.BossCombat,
                0,
                0,
                BossStatus.Alive);

            GameState state = _factory.CreateState(
                player,
                _factory.CreateEnemies(boss),
                wave);

            FrameInput input = new(
                Direction3D.Zero,
                Position3D.Zero,
                Direction3D.Right,
                true,
                false);

            SimulationPlan plan = Simulation.Plan(
                state,
                _config,
                input,
                0.1f);

            ResolvedSimulationContext context = new(
                    MovementResolution.Unblocked,
                    AttackResolution.RangedHit(bossId),
                    SpawnResolution.None);

            SimulationOutput output = Simulation.Execute(
                state,
                _config,
                input,
                plan,
                context,
                0.1f);

            Assert.That(output.State.Phase, Is.EqualTo(GamePhase.Victory));
            Assert.That(output.State.CurrentWave.Phase, Is.EqualTo(WavePhase.Completed));
            Assert.That(output.State.CurrentWave.BossStatus, Is.EqualTo(BossStatus.Defeated));
            Assert.That(output.Result.GamePhaseChanged, Is.True);
            Assert.That(output.Result.GamePhase, Is.EqualTo(GamePhase.Victory));
        }
    }
}

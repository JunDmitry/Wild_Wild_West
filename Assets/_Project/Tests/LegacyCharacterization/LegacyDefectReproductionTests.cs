using System;
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
    [Category("LegacyDefect")]
    public sealed class LegacyDefectReproductionTests
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
        public void ExistingBehaviorSwitchAndAttackUsesInconsistentWeaponStates()
        {
            GameState state = GameInit.NewGame(
                _config,
                new EntityId(1),
                1);

            FrameInput input = new(
                Direction3D.Zero,
                Position3D.Zero,
                Direction3D.Forward,
                true,
                true);

            SimulationPlan plan = Simulation.Plan(
                state,
                _config,
                input,
                0.1f);

            ResolvedSimulationContext context = new(
                    MovementResolution.Unblocked,
                    AttackResolution.MeleeHits(Array.Empty<EntityId>()),
                    SpawnResolution.None);

            SimulationOutput output = Simulation.Execute(
                state,
                _config,
                input,
                plan,
                context,
                0.1f);

            Assert.That(plan.Attack.Kind, Is.EqualTo(AttackQueryKind.MeleeOverlap));
            Assert.That(output.Result.SelectedWeapon, Is.EqualTo(WeaponKind.Melee));
            Assert.That(output.Result.AttackWeapon, Is.EqualTo(WeaponKind.Ranged));
            Assert.That(output.State.Player.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
            Assert.That(output.State.Player.RangedReadyTime, Is.EqualTo(0.5f));
            Assert.That(output.State.Player.MeleeReadyTime, Is.EqualTo(0f));
        }

        [Test]
        public void ExistingBehaviorMissingSpawnCreatesDefaultSpawnFact()
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

            Assert.That(output.State.Enemies, Is.Empty);
            Assert.That(output.Result.EnemiesSpawned.Count, Is.EqualTo(1));
            Assert.That(output.Result.EnemiesSpawned[0].Id, Is.EqualTo(EntityId.None));
        }

        [Test]
        public void ExistingBehaviorDuplicateMeleeHitsDamageEnemyMoreThanOnce()
        {
            EntityId enemyId = new(2);

            PlayerState player = _factory.CreatePlayer(
                new EntityId(1),
                WeaponKind.Melee);

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
                true,
                false);

            SimulationPlan plan = Simulation.Plan(
                state,
                _config,
                input,
                0.1f);

            EntityId[] hits =
            {
                enemyId,
                enemyId,
            };

            ResolvedSimulationContext context = new(
                    MovementResolution.Unblocked,
                    AttackResolution.MeleeHits(hits),
                    SpawnResolution.None);

            SimulationOutput output = Simulation.Execute(
                state,
                _config,
                input,
                plan,
                context,
                0.1f);

            Assert.That(output.State.Enemies[enemyId].CurrentHealth, Is.EqualTo(15f));
            Assert.That(output.Result.EnemiesDamaged.Count, Is.EqualTo(2));
        }

        [Test]
        public void ExistingBehaviorExecutingCompletedWaveThrows()
        {
            PlayerState player = _factory.CreatePlayer(
                new EntityId(1),
                WeaponKind.Ranged);

            WaveState wave = new(
                1,
                WavePhase.Completed,
                0,
                0,
                BossStatus.Defeated);

            GameState state = _factory.CreateState(
                player,
                new Dictionary<EntityId, EnemyState>(),
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

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    Simulation.Execute(
                        state,
                        _config,
                        input,
                        plan,
                        ResolvedSimulationContext.Empty,
                        0.1f);
                });
        }
    }
}

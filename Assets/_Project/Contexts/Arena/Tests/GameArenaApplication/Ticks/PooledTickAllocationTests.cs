using System;
using System.Collections.Generic;
using Game.Arena.Application.Input;
using Game.Arena.Application.Sessions;
using Game.Arena.Application.Tests.Sessions;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Attack;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Ticks
{
    /// <summary>
    /// ADR-0035 requirement 13 / ADR-0036: the tick path must allocate nothing
    /// once its buffers are warm. These tests use
    /// <see cref="GC.GetAllocatedBytesForCurrentThread"/>, which works in the
    /// editor and in the standalone test host but not in a player build.
    /// </summary>
    [TestFixture]
    public sealed class PooledTickAllocationTests
    {
        private const int MeasuredTicks = 300;
        private const int WarmUpTicks = 50;

        [Test]
        public void SteadyStateTicks_AllocateNothing()
        {
            TickCoordinatorTestKit kit = new();
            kit.EnemyMovementResolver.Mode = ScriptedEnemyMovementResolver.ResolveMode.KeepInPlace;
            kit.Input.Input = IdleInput();
            kit.Session.StartInitialRun();

            ArenaRun run = kit.Session.GetRequiredActiveRun();
            int spawnTicks = 0;

            while (run.ActiveEnemyCount < 10 && spawnTicks < 1000)
            {
                kit.Coordinator.ExecuteTick();
                spawnTicks++;
            }

            Assert.That(
                run.ActiveEnemyCount,
                Is.EqualTo(10),
                "ten enemies must be active before the measurement starts");

            for (int index = 0; index < WarmUpTicks; index++)
            {
                kit.Coordinator.ExecuteTick();
            }

            long before = GC.GetAllocatedBytesForCurrentThread();

            for (int index = 0; index < MeasuredTicks; index++)
            {
                kit.Coordinator.ExecuteTick();
            }

            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(
                allocated,
                Is.EqualTo(0),
                "a steady-state tick with ten active enemies must allocate nothing");
        }

        [Test]
        public void MultiEventAttackFrame_AllocatesNothing()
        {
            // The first pass warms process-wide caches (generic comparers,
            // JIT of the event types); the measured pass is the steady state.
            RunMultiEventScenario(measure: false);
            long allocated = RunMultiEventScenario(measure: true);

            Assert.That(
                allocated,
                Is.EqualTo(0),
                "a multi-event attack frame must reuse the per-type event pool");
        }

        private static long RunMultiEventScenario(bool measure)
        {
            TickCoordinatorTestKit kit = new(
                creationParameters: CreateLethalMeleeParameters());
            kit.Session.StartInitialRun();

            ArenaRun run = kit.Session.GetRequiredActiveRun();

            // Warm-up: wave one is cleared so every pooled event type this
            // frame needs has a live instance in the slot.
            List<EnemyId> waveOne = SpawnRegulars(run, 10, 1);
            SwingMelee(run, waveOne);
            Assert.That(WaveNumberOf(run), Is.EqualTo(WaveNumber.First));

            run.AdvanceTime(new GameDuration(1d));
            EnemyId bossId = SpawnBoss(run, 500UL);
            SwingMelee(run, new List<EnemyId> { bossId });
            Assert.That(WaveNumberOf(run), Is.EqualTo(WaveNumber.First.Next()));

            // Reuse slot A: two toggles release it, keeping its instances.
            run.BeginTick();
            run.BeginTick();

            List<EnemyId> waveTwo = SpawnRegulars(run, 10, 11);
            run.AdvanceTime(new GameDuration(1d));
            BeginAttack(run);

            PlayerAttackImpactRequest measuredRequest =
                run.RequestPlayerAttackImpact(Direction3D.Right).Request;
            PlayerAttackImpactResolution measuredResolution =
                new PlayerAttackImpactResolution(
                    measuredRequest.Correlation,
                    waveTwo);

            long before = GC.GetAllocatedBytesForCurrentThread();
            PlayerAttackImpactOutcome outcome = run.ApplyPlayerAttackImpact(measuredResolution);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(
                outcome.Change.DomainEvents.Count,
                Is.GreaterThanOrEqualTo(20),
                "the frame must carry ten EnemyDamaged and ten EnemyDefeated events");

            return measure ? allocated : 0;
        }

        private static PlayerFrameInput IdleInput()
        {
            return new PlayerFrameInput(
                MovementAxes.Zero,
                Direction3D.Right,
                false,
                false);
        }

        private static ArenaRunCreationParameters CreateLethalMeleeParameters()
        {
            ArenaRunCreationParameters defaults =
                new ArenaRunCreationParametersFactory().Create();

            EnemyCatalog enemies = new(
                new EnemyDefinition(
                    EnemyKind.Regular,
                    Health.Full(50),
                    MovementSpeed.FromUnitsPerSecond(2f),
                    CollisionRadius.FromValue(0.5f),
                    DamageAmount.FromPoints(5),
                    Distance.FromValue(2f),
                    new GameDuration(1d),
                    new GameDuration(0.2d)),
                new EnemyDefinition(
                    EnemyKind.Boss,
                    Health.Full(60),
                    MovementSpeed.FromUnitsPerSecond(1.75f),
                    CollisionRadius.FromValue(1f),
                    DamageAmount.FromPoints(25),
                    Distance.FromValue(3f),
                    new GameDuration(1.1d),
                    new GameDuration(0.4d)));

            WeaponCatalog weapons = new(
                new WeaponDefinition(
                    WeaponKind.Ranged,
                    DamageAmount.FromPoints(10),
                    Distance.FromValue(50f),
                    new GameDuration(0.4d),
                    new GameDuration(0d)),
                new WeaponDefinition(
                    WeaponKind.Melee,
                    DamageAmount.FromPoints(60),
                    Distance.FromValue(2f),
                    new GameDuration(0.8d),
                    new GameDuration(0.3d)));

            WaveCatalog waves = new(
                new[]
                {
                    new WaveDefinition(WaveNumber.First, 10),
                    new WaveDefinition(WaveNumber.First.Next(), 10),
                });

            return new ArenaRunCreationParameters(
                defaults.ArenaDefinition,
                new PlayerDefinition(
                    new Position3D(19.5f, 0f, 0f),
                    defaults.PlayerDefinition.InitialHealth,
                    defaults.PlayerDefinition.MovementSpeed,
                    defaults.PlayerDefinition.CollisionRadius),
                weapons,
                enemies,
                waves);
        }

        private static List<EnemyId> SpawnRegulars(
            ArenaRun run,
            int count,
            int firstValue)
        {
            List<EnemyId> ids = new(count);

            for (int index = 0; index < count; index++)
            {
                float x = 21f + (index * 0.05f);
                EnemySpawnRequest request = run.RequestEnemySpawn().Request;
                EnemyId enemyId = EnemyId.FromValue((ulong)(firstValue + index));
                EnemySpawnResolutionOutcome outcome = run.ApplyEnemySpawn(
                    new EnemySpawnResolution(request.Correlation, enemyId, new Position3D(x, 0f, 0f)));

                Assert.That(
                    outcome.IsSpawned,
                    Is.True,
                    "spawn rejected: " + outcome.RejectionReason);

                ids.Add(enemyId);
            }

            return ids;
        }

        private static EnemyId SpawnBoss(
            ArenaRun run,
            ulong value)
        {
            EnemySpawnRequest request = run.RequestEnemySpawn().Request;
            EnemyId enemyId = EnemyId.FromValue(value);
            EnemySpawnResolutionOutcome outcome = run.ApplyEnemySpawn(
                new EnemySpawnResolution(request.Correlation, enemyId, new Position3D(21f, 0f, 0f)));

            Assert.That(
                outcome.IsSpawned,
                Is.True,
                "boss spawn rejected: " + outcome.RejectionReason);

            return enemyId;
        }

        private static void SwingMelee(ArenaRun run, List<EnemyId> targets)
        {
            BeginAttack(run);
            ApplyMeleeImpact(run, targets);
        }

        private static void BeginAttack(ArenaRun run)
        {
            if (run.SelectedWeapon != WeaponKind.Melee)
            {
                WeaponSwitchOutcome switchOutcome = run.SwitchWeapon();

                Assert.That(
                    switchOutcome.IsSwitched,
                    Is.True,
                    "weapon switch did not happen: " + switchOutcome.Status);
            }

            PlayerAttackStartOutcome startOutcome = run.StartPlayerAttack();

            Assert.That(
                startOutcome.IsStarted,
                Is.True,
                "attack did not start: " + startOutcome.Status);

            run.AdvanceTime(new GameDuration(0.3d));
        }

        private static PlayerAttackImpactOutcome ApplyMeleeImpact(
            ArenaRun run,
            List<EnemyId> targets)
        {
            PlayerAttackImpactRequest request =
                run.RequestPlayerAttackImpact(Direction3D.Right).Request;
            PlayerAttackImpactResolution resolution =
                new PlayerAttackImpactResolution(request.Correlation, targets);

            return run.ApplyPlayerAttackImpact(resolution);
        }

        private static WaveNumber WaveNumberOf(ArenaRun run)
        {
            return run.CurrentWaveNumber;
        }
    }
}

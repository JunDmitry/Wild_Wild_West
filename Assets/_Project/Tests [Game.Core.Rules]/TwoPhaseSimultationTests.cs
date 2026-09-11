using System.Collections;
using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Initializers;
using Game.Core.Model.Results;
using Game.Core.Model.Simulation.Queries;
using Game.Core.Model.Simulation.Resolutions;
using Game.Core.Rules;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class TwoPhaseSimultationTests
{
    private GameConfig _config;

    [SetUp]
    public void Setup()
    {
        ArenaConfig arena = new(-20f, 20f, -20f, 20f, 0f, 0.5f);
        PlayerConfig player = new(100, Position3D.Zero, 0f, 0f, 5f);
        WeaponConfig ranged = new(WeaponKind.Ranged, 0.4f, 10f, 100f);
        WeaponConfig melee = new(WeaponKind.Melee, 0.8f, 17.5f, 5f);
        EnemyConfig regular = new(EnemyKind.Regular, 50, 2f, 7.5f, 2f, 1f);
        EnemyConfig boss = new(EnemyKind.Boss, 300, 1.75f, 25f, 3f, 1.1f);
        WaveConfig[] waves = new[]
        {
            new WaveConfig(1, 2, hasBoss: false),
            new WaveConfig(2, 1, hasBoss: true)
        };

        _config = new GameConfig(arena, player, ranged, melee, regular, boss, waves, restartDelay: 2f);
    }

    [Test]
    public void Plan_WhenMoving_EmitsMovementQuery()
    {
        Game.Core.Model.States.GameState state = GameInit.NewGame(_config, new EntityId(1), 1);
        FrameInput input = new(
            new Direction3D(1f, 0f, 0f),
            Position3D.Zero,
            Direction3D.Forward,
            attackPressed: false,
            switchWeaponPressed: false);

        SimulationPlan plan = Simulation.Plan(state, _config, input, delta: 0.1f);

        Assert.That(plan.HasMovementQuery, Is.True);
        Assert.That(plan.Movement.From, Is.EqualTo(Position3D.Zero));
        Assert.That(plan.Movement.To.X, Is.GreaterThan(0f));
    }

    [Test]
    public void Plan_WhenSwitchAndAttackPressedInSameFrame_PlansWithSwitchedWeapon()
    {
        Game.Core.Model.States.GameState state = GameInit.NewGame(_config, new EntityId(1), 1);
        FrameInput input = new(
            Direction3D.Zero,
            Position3D.Zero,
            Direction3D.Forward,
            attackPressed: true,
            switchWeaponPressed: true);

        SimulationPlan plan = Simulation.Plan(state, _config, input, delta: 0.1f);

        Assert.That(plan.Attack.Kind, Is.EqualTo(AttackQueryKind.MeleeOverlap));
        Assert.That(plan.Attack.Range, Is.EqualTo(_config.MeleeWeapon.Range));
    }

    [Test]
    public void Plan_WhenRegularEnemiesRemain_EmitsSpawnQueryRegular()
    {
        Game.Core.Model.States.GameState state = GameInit.NewGame(_config, new EntityId(1), 1);
        FrameInput input = new(Direction3D.Zero, Position3D.Zero, Direction3D.Forward, false, false);

        SimulationPlan plan = Simulation.Plan(state, _config, input, delta: 0.1f);

        Assert.That(plan.Spawn.Kind, Is.EqualTo(SpawnQueryKind.Regular));
    }

    [Test]
    public void Execute_WithSpawnResolution_SpawnsEnemyWithExplicitId()
    {
        Game.Core.Model.States.GameState state = GameInit.NewGame(_config, new EntityId(1), 1);
        FrameInput input = new(Direction3D.Zero, Position3D.Zero, Direction3D.Forward, false, false);
        SimulationPlan plan = Simulation.Plan(state, _config, input, delta: 0.1f);

        EntityId reservedId = new(42);
        Position3D spawnPos = new(10f, 0f, 10f);
        ResolvedSimulationContext context = new(
            MovementResolution.Unblocked,
            AttackResolution.Empty,
            SpawnResolution.Successful(reservedId, spawnPos));

        SimulationOutput output = Simulation.Execute(state, _config, input, plan, context, delta: 0.1f);

        Assert.That(output.State.Enemies.ContainsKey(reservedId), Is.True);
        Assert.That(output.State.Enemies[reservedId].Position, Is.EqualTo(spawnPos));
        Assert.That(output.Result.EnemiesSpawned.Count, Is.EqualTo(1));
        Assert.That(output.Result.EnemiesSpawned[0].Id, Is.EqualTo(reservedId));
    }
}

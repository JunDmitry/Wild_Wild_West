using System.Collections.Generic;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.States;
using NUnit.Framework;

[TestFixture]
public class GameStateEqualityTests
{
    [Test]
    public void IdenticalStates_AreEqual()
    {
        PlayerState player = new PlayerState(
            EntityId.None,
            new Position3D(0, 0, 0),
            100, 100,
            WeaponKind.Ranged,
            0f, 0f);

        WaveState wave = new WaveState(1, WavePhase.RegularCombat, 5, 0, BossStatus.NotSpawned);
        Dictionary<EntityId, EnemyState> enemies = new Dictionary<EntityId, EnemyState>();

        GameState state = new GameState(
            GamePhase.Playing, player, enemies, wave, 0f, 0f);

        Assert.That(state.Equals(state), Is.True);
    }

    [Test]
    public void DifferentPlayerHealth_StatesDiffer()
    {
        PlayerState leftPlayer = new PlayerState(
            EntityId.None,
            new Position3D(0, 0, 0),
            100, 100,
            WeaponKind.Ranged,
            0f, 0f);

        PlayerState rightPlayer = new PlayerState(
            EntityId.None,
            new Position3D(0, 0, 0),
            90, 100,
            WeaponKind.Ranged,
            0f, 0f);

        WaveState wave = new WaveState(1, WavePhase.RegularCombat, 5, 0, BossStatus.NotSpawned);
        Dictionary<EntityId, EnemyState> enemies = new Dictionary<EntityId, EnemyState>();

        GameState left = new GameState(GamePhase.Playing, leftPlayer, enemies, wave, 0f, 0f);
        GameState right = new GameState(GamePhase.Playing, rightPlayer, enemies, wave, 0f, 0f);

        Assert.That(left.Equals(right), Is.False);
    }
}

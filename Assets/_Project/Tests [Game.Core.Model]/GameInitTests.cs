using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Initializers;
using NUnit.Framework;

[TestFixture]
public class GameInitTests
{
    [Test]
    public void NewGame_ContainsExpectedPlayer()
    {
        GameConfig config = CreateValidConfig();
        Game.Core.Model.States.GameState gameState = GameInit.NewGame(config, EntityId.None, waveNumber: 1);

        Assert.That(gameState.Player.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
        Assert.That(gameState.Player.MaxHealth, Is.EqualTo(100));
        Assert.That(gameState.Player.CurrentHealth, Is.EqualTo(100));
    }

    [Test]
    public void NewGame_PhaseIsPlaying()
    {
        GameConfig config = CreateValidConfig();
        Game.Core.Model.States.GameState gameState = GameInit.NewGame(config, EntityId.None, waveNumber: 1);

        Assert.That(gameState.Phase, Is.EqualTo(GamePhase.Playing));
    }

    [Test]
    public void NewGame_WaveIsInRegularCombat()
    {
        GameConfig config = CreateValidConfig();
        Game.Core.Model.States.GameState gameState = GameInit.NewGame(config, EntityId.None, waveNumber: 1);

        Assert.That(gameState.CurrentWave.Phase, Is.EqualTo(WavePhase.RegularCombat));
        Assert.That(gameState.CurrentWave.RegularToSpawn, Is.EqualTo(5));
        Assert.That(gameState.CurrentWave.RegularAlive, Is.Zero);
    }

    [Test]
    public void NewGame_NoEnemies()
    {
        GameConfig config = CreateValidConfig();
        Game.Core.Model.States.GameState gameState = GameInit.NewGame(config, EntityId.None, waveNumber: 1);

        Assert.That(gameState.Enemies, Is.Empty);
    }

    [Test]
    public void NewGame_TimeIsZero()
    {
        GameConfig config = CreateValidConfig();
        Game.Core.Model.States.GameState gameState = GameInit.NewGame(config, EntityId.None, waveNumber: 1);

        Assert.That(gameState.Time, Is.EqualTo(0f));
    }

    [Test]
    public void NewGame_WaveNumberCountsFromConfig()
    {
        GameConfig config = CreateValidConfig();
        Game.Core.Model.States.GameState gameState = GameInit.NewGame(config, EntityId.None, waveNumber: 3);

        Assert.That(gameState.CurrentWave.Number, Is.EqualTo(3));
    }

    private GameConfig CreateValidConfig()
    {
        ArenaConfig arena = new ArenaConfig(
            minX: -20f,
            maxX: 20f,
            minZ: -20f,
            maxZ: 20f,
            groundY: 0f,
            playerRadius: 0.5f);

        PlayerConfig player = new PlayerConfig(
            maxHealth: 100,
            startPosition: Position3D.Zero,
            startRangedReadyTime: 0f,
            startMeleeReadyTime: 0f,
            moveSpeed: 5f);

        WeaponConfig ranged = new WeaponConfig(WeaponKind.Ranged, cooldown: 0.4f, damage: 10f, range: 100f);
        WeaponConfig melee = new WeaponConfig(WeaponKind.Melee, cooldown: 0.8f, damage: 17.5f, range: 20f);

        EnemyConfig regular = new EnemyConfig(EnemyKind.Regular, maxHealth: 50, moveSpeed: 2, attackDamage: 7.5f, attackRange: 17.5f, attackCooldown: 1f);
        EnemyConfig boss = new EnemyConfig(EnemyKind.Boss, maxHealth: 300, moveSpeed: 1.75f, attackDamage: 25f, attackRange: 20f, attackCooldown: 1.1f);

        WaveConfig[] waves = new WaveConfig[]
        {
            new WaveConfig(1, 5, hasBoss: true),
            new WaveConfig(2, 8, hasBoss: true),
            new WaveConfig(3, 12, hasBoss: true)
        };

        return new GameConfig(
            arena,
            player,
            ranged,
            melee,
            regular,
            boss,
            waves,
            restartDelay: 2f);
    }
}

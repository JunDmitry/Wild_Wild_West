using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using NUnit.Framework;

[TestFixture]
public class GameConfigTests
{
    [Test]
    public void Config_RoundTrip_Equals()
    {
        GameConfig config = CreateValidConfig();

        bool areEquals = config.Equals(config);

        Assert.That(areEquals, Is.True);
    }

    [Test]
    public void ConfigHash_Consistent_Equals()
    {
        GameConfig config = CreateValidConfig();

        int hash1 = config.GetHashCode();
        int hash2 = config.GetHashCode();

        Assert.That(hash1, Is.EqualTo(hash2));
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

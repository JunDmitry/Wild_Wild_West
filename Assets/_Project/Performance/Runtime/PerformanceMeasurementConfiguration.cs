using Game.Arena.Application.Diagnostics;
using Game.Arena.Application.Sessions;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Vitality;
using Game.CompositionRoot;

namespace Game.Performance
{
    /// <summary>
    /// The measurement catalogue (ADR-0034). It is the shipped default with the
    /// player's and both enemies' health raised so a 60 s sample ends neither in
    /// defeat nor in victory; every other field is copied unchanged. The boss is
    /// one point above the regular enemy because the domain's catalogue requires
    /// boss health to exceed every regular enemy's health.
    /// </summary>
    public static class PerformanceMeasurementConfiguration
    {
        public const int MeasurementHealth = 1000000;

        public static ArenaGameConfiguration Create()
        {
            ArenaGameConfiguration shipped = ArenaGameConfiguration.CreateDefault();
            ArenaRunCreationParameters shippedParameters = shipped.CreationParameters;

            PlayerDefinition player = new(
                shippedParameters.PlayerDefinition.StartPosition,
                Health.Full(MeasurementHealth),
                shippedParameters.PlayerDefinition.MovementSpeed,
                shippedParameters.PlayerDefinition.CollisionRadius);

            EnemyCatalog enemies = new(
                RaiseHealth(
                    shippedParameters.EnemyCatalog.Get(EnemyKind.Regular),
                    MeasurementHealth),
                RaiseHealth(
                    shippedParameters.EnemyCatalog.Get(EnemyKind.Boss),
                    MeasurementHealth + 1));

            // TEMPORARY attribution switch (t9-gc-residual-b); reverted before
            // the acceptance runs. The override keeps one regular enemy per
            // wave to test the per-enemy-frame hypothesis.
            int? waveEnemies = AttributionFlags.WaveEnemies;
            WaveCatalog waves = waveEnemies.HasValue
                ? ResizeWaves(shippedParameters.WaveCatalog, waveEnemies.Value)
                : shippedParameters.WaveCatalog;

            ArenaRunCreationParameters measurementParameters = new(
                shippedParameters.ArenaDefinition,
                player,
                shippedParameters.WeaponCatalog,
                enemies,
                waves);

            return new ArenaGameConfiguration(
                shipped.Arena,
                measurementParameters,
                shipped.RestartDelay,
                shipped.SpawnInterval,
                shipped.ArenaSceneName);
        }

        private static WaveCatalog ResizeWaves(WaveCatalog shipped, int regularEnemyCount)
        {
            System.Collections.Generic.List<WaveDefinition> resized = new();

            for (WaveNumber number = WaveNumber.First;
                 number.Value <= shipped.Last.Value;
                 number = number.Next())
            {
                resized.Add(new WaveDefinition(number, regularEnemyCount));
            }

            return new WaveCatalog(resized);
        }

        private static EnemyDefinition RaiseHealth(
            EnemyDefinition definition,
            int health)
        {
            return new EnemyDefinition(
                definition.Kind,
                Health.Full(health),
                definition.MovementSpeed,
                definition.CollisionRadius,
                definition.AttackDamage,
                definition.AttackRange,
                definition.AttackCooldown,
                definition.AttackWindupDuration);
        }
    }
}

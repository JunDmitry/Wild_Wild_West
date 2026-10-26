using Game.Arena.Application.Sessions;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Configuration;
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

            ArenaRunCreationParameters measurementParameters = new(
                shippedParameters.ArenaDefinition,
                player,
                shippedParameters.WeaponCatalog,
                enemies,
                shippedParameters.WaveCatalog);

            return new ArenaGameConfiguration(
                shipped.Arena,
                measurementParameters,
                shipped.RestartDelay,
                shipped.SpawnInterval,
                shipped.ArenaSceneName);
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

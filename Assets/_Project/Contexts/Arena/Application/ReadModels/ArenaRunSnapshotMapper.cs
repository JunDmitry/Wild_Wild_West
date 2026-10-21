using System;
using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Application.ReadModels
{
    public sealed class ArenaRunSnapshotMapper
    {
        public ArenaRunSnapshot Map(ArenaRunStateSnapshot source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            PlayerSnapshot player = new(
                source.Player.PlayerId,
                source.Player.Position,
                source.Player.Health,
                source.Player.SelectedWeapon,
                source.Player.RangedReadyAt,
                source.Player.MeleeReadyAt,
                source.Player.HasPendingAttack,
                source.Player.PendingAttack);

            EnemySnapshot[] enemies = new EnemySnapshot[source.Enemies.Count];

            for (int index = 0; index < source.Enemies.Count; index++)
            {
                EnemyStateSnapshot enemy = source.Enemies[index];

                enemies[index] = new EnemySnapshot(
                    enemy.EnemyId,
                    enemy.Kind,
                    enemy.Position,
                    enemy.Health,
                    enemy.CollisionRadius,
                    enemy.AttackReadyAt,
                    enemy.HasPendingAttack,
                    enemy.PendingAttack);
            }

            WaveSnapshot wave = new(
                source.Wave.Number,
                source.Wave.Phase,
                source.Wave.RegularEnemiesRemainingToSpawn,
                source.Wave.BossStatus);

            return new ArenaRunSnapshot(
                source.ArenaRunId,
                source.Revision,
                source.Status,
                source.CurrentTime,
                source.Bounds,
                player,
                enemies,
                wave);
        }
    }
}

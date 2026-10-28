using System;
using System.Collections.Generic;
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

            ArenaRunSnapshot target = new(source.Enemies.Count);
            Map(source, target);

            return target;
        }

        internal void Map(ArenaRunStateSnapshot source, ArenaRunSnapshot target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Player.Fill(
                source.Player.PlayerId,
                source.Player.Position,
                source.Player.Health,
                source.Player.SelectedWeapon,
                source.Player.RangedReadyAt,
                source.Player.MeleeReadyAt,
                source.Player.HasPendingAttack,
                source.Player.PendingAttack);

            List<EnemySnapshot> enemies = target.EnemyBuffer;
            enemies.Clear();

            for (int index = 0; index < source.Enemies.Count; index++)
            {
                EnemyStateSnapshot sourceEnemy = source.Enemies[index];
                EnemySnapshot enemy = target.GetOrCreateEnemySlot(index);

                enemy.Fill(
                    sourceEnemy.EnemyId,
                    sourceEnemy.Kind,
                    sourceEnemy.Position,
                    sourceEnemy.Health,
                    sourceEnemy.CollisionRadius,
                    sourceEnemy.AttackReadyAt,
                    sourceEnemy.HasPendingAttack,
                    sourceEnemy.PendingAttack);

                enemies.Add(enemy);
            }

            target.Wave.Fill(
                source.Wave.Number,
                source.Wave.Phase,
                source.Wave.RegularEnemiesRemainingToSpawn,
                source.Wave.BossStatus);

            target.FillMeta(
                source.ArenaRunId,
                source.Revision,
                source.Status,
                source.CurrentTime,
                source.Bounds,
                source.WaveTotal);
        }
    }
}

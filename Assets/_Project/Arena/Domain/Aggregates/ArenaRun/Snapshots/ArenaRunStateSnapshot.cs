using System;
using System.Collections.Generic;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRunStateSnapshot
    {
        public ArenaRunStateSnapshot(
            ArenaRunId arenaRunId,
            AggregateRevision revision,
            ArenaRunStatus status,
            GameTimePoint currentTime,
            ArenaBounds bounds,
            PlayerStateSnapshot player,
            IReadOnlyList<EnemyStateSnapshot> enemies,
            WaveStateSnapshot wave)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (enemies == null)
            {
                throw new ArgumentNullException(nameof(enemies));
            }

            if (wave == null)
            {
                throw new ArgumentNullException(nameof(wave));
            }

            ArenaRunId = arenaRunId;
            Revision = revision;
            Status = status;
            CurrentTime = currentTime;
            Bounds = bounds;
            Player = player;
            Enemies = CopyEnemies(enemies);
            Wave = wave;
        }

        public ArenaRunId ArenaRunId { get; }

        public AggregateRevision Revision { get; }

        public ArenaRunStatus Status { get; }

        public GameTimePoint CurrentTime { get; }

        public ArenaBounds Bounds { get; }

        public PlayerStateSnapshot Player { get; }

        public IReadOnlyList<EnemyStateSnapshot> Enemies { get; }

        public WaveStateSnapshot Wave { get; }

        private static IReadOnlyList<EnemyStateSnapshot> CopyEnemies(
            IReadOnlyList<EnemyStateSnapshot> enemies)
        {
            EnemyStateSnapshot[] copied = new EnemyStateSnapshot[enemies.Count];

            for (int index = 0; index < enemies.Count; index++)
            {
                EnemyStateSnapshot enemy = enemies[index]
                    ?? throw new ArgumentException("Enemy snapshots cannot contain null values.", nameof(enemies));

                copied[index] = enemy;
            }

            return Array.AsReadOnly(copied);
        }
    }
}

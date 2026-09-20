using System;
using System.Collections.Generic;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.ReadModels
{
    public sealed class ArenaRunSnapshot
    {
        public ArenaRunSnapshot(
            ArenaRunId arenaRunId,
            AggregateRevision revision,
            ArenaRunStatus status,
            GameTimePoint currentTime,
            ArenaBounds bounds,
            PlayerSnapshot player,
            IReadOnlyList<EnemySnapshot> enemies,
            WaveSnapshot wave)
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

        public PlayerSnapshot Player { get; }

        public IReadOnlyList<EnemySnapshot> Enemies { get; }

        public WaveSnapshot Wave { get; }

        private static IReadOnlyList<EnemySnapshot> CopyEnemies(
            IReadOnlyList<EnemySnapshot> enemies)
        {
            EnemySnapshot[] copied = new EnemySnapshot[enemies.Count];

            for (int index = 0; index < enemies.Count; index++)
            {
                EnemySnapshot enemy = enemies[index]
                    ?? throw new ArgumentException("Enemy snapshots cannot contain null values.", nameof(enemies));

                copied[index] = enemy;
            }

            return Array.AsReadOnly(copied);
        }
    }
}

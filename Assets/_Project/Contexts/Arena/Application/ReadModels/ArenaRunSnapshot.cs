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
        private readonly List<EnemySnapshot> _enemyBuffer;
        private EnemySnapshot[] _enemySlots;

        internal ArenaRunSnapshot(int enemyCapacity)
        {
            if (enemyCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(enemyCapacity));
            }

            _enemyBuffer = new List<EnemySnapshot>(enemyCapacity);
            _enemySlots = new EnemySnapshot[enemyCapacity];
            Player = new PlayerSnapshot();
            Wave = new WaveSnapshot();
            Enemies = _enemyBuffer.AsReadOnly();
        }

        public ArenaRunSnapshot(
            ArenaRunId arenaRunId,
            AggregateRevision revision,
            ArenaRunStatus status,
            GameTimePoint currentTime,
            ArenaBounds bounds,
            PlayerSnapshot player,
            IReadOnlyList<EnemySnapshot> enemies,
            WaveSnapshot wave,
            WaveNumber waveTotal)
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

            if (waveTotal < wave.Number)
            {
                throw new ArgumentException("Wave total cannot precede the current wave.", nameof(waveTotal));
            }

            ArenaRunId = arenaRunId;
            Revision = revision;
            Status = status;
            CurrentTime = currentTime;
            Bounds = bounds;
            Player = player;
            Enemies = CopyEnemies(enemies);
            Wave = wave;
            WaveTotal = waveTotal;
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision Revision { get; private set; }

        public ArenaRunStatus Status { get; private set; }

        public GameTimePoint CurrentTime { get; private set; }

        public ArenaBounds Bounds { get; private set; }

        public PlayerSnapshot Player { get; private set; }

        public IReadOnlyList<EnemySnapshot> Enemies { get; private set; }

        public WaveSnapshot Wave { get; private set; }

        public WaveNumber WaveTotal { get; private set; }

        internal List<EnemySnapshot> EnemyBuffer => _enemyBuffer;

        internal EnemySnapshot GetOrCreateEnemySlot(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index >= _enemySlots.Length)
            {
                EnemySnapshot[] grown = new EnemySnapshot[index + 1];
                Array.Copy(_enemySlots, grown, _enemySlots.Length);
                _enemySlots = grown;
            }

            EnemySnapshot slot = _enemySlots[index];

            if (slot == null)
            {
                slot = new EnemySnapshot();
                _enemySlots[index] = slot;
            }

            return slot;
        }

        internal void FillMeta(
            ArenaRunId arenaRunId,
            AggregateRevision revision,
            ArenaRunStatus status,
            GameTimePoint currentTime,
            ArenaBounds bounds,
            WaveNumber waveTotal)
        {
            ArenaRunId = arenaRunId;
            Revision = revision;
            Status = status;
            CurrentTime = currentTime;
            Bounds = bounds;
            WaveTotal = waveTotal;
        }

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

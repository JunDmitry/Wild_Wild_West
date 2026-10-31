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
        private readonly List<EnemyStateSnapshot> _enemyBuffer;
        private EnemyStateSnapshot[] _enemySlots;

        public ArenaRunStateSnapshot(
            ArenaRunId arenaRunId,
            AggregateRevision revision,
            ArenaRunStatus status,
            GameTimePoint currentTime,
            ArenaBounds bounds,
            PlayerStateSnapshot player,
            IReadOnlyList<EnemyStateSnapshot> enemies,
            WaveStateSnapshot wave,
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
            PlayerSlot = player;
            Enemies = CopyEnemies(enemies);
            WaveSlot = wave;
            WaveTotal = waveTotal;
        }

        internal ArenaRunStateSnapshot(int enemyCapacity)
        {
            if (enemyCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(enemyCapacity));
            }

            _enemyBuffer = new List<EnemyStateSnapshot>(enemyCapacity);
            _enemySlots = new EnemyStateSnapshot[enemyCapacity];
            PlayerSlot = new PlayerStateSnapshot();
            WaveSlot = new WaveStateSnapshot();
            Enemies = _enemyBuffer.AsReadOnly();
        }

        public ArenaRunId ArenaRunId { get; private set; }

        public AggregateRevision Revision { get; private set; }

        public ArenaRunStatus Status { get; private set; }

        public GameTimePoint CurrentTime { get; private set; }

        public ArenaBounds Bounds { get; private set; }

        public PlayerStateSnapshot Player => PlayerSlot;

        public IReadOnlyList<EnemyStateSnapshot> Enemies { get; private set; }

        public WaveStateSnapshot Wave => WaveSlot;

        public WaveNumber WaveTotal { get; private set; }

        internal PlayerStateSnapshot PlayerSlot { get; }

        internal WaveStateSnapshot WaveSlot { get; }

        internal List<EnemyStateSnapshot> EnemyBuffer => _enemyBuffer;

        internal EnemyStateSnapshot GetOrCreateEnemySlot(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index >= _enemySlots.Length)
            {
                EnemyStateSnapshot[] grown = new EnemyStateSnapshot[index + 1];
                Array.Copy(_enemySlots, grown, _enemySlots.Length);
                _enemySlots = grown;
            }

            EnemyStateSnapshot slot = _enemySlots[index];

            if (slot == null)
            {
                slot = new EnemyStateSnapshot();
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

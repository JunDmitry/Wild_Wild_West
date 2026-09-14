using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRun
    {
        private readonly Player _player;
        private readonly Wave _currentWave;
        private readonly ArenaBounds _arenaBounds;

        internal ArenaRun(
            ArenaRunId id,
            Player player,
            Wave currentWave,
            ArenaBounds arenaBounds)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(id));
            }

            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (arenaBounds.CanContain(player.CollisionRadius) == false)
            {
                throw new ArgumentException("Arena cannot contain the Player.", nameof(arenaBounds));
            }

            if (arenaBounds.Contains(player.Position, player.CollisionRadius) == false)
            {
                throw new ArgumentException("Player position is outside the Arena.", nameof(player));
            }

            Id = id;
            _player = player;
            _currentWave = currentWave ?? throw new ArgumentNullException(nameof(currentWave));
            _arenaBounds = arenaBounds;
            Status = ArenaRunStatus.Playing;
            Revision = AggregateRevision.Initial;
        }

        public ArenaRunId Id { get; }
        public ArenaRunStatus Status { get; private set; }
        public AggregateRevision Revision { get; private set; }
        public PlayerId PlayerId => _player.Id;
        public Position3D PlayerPosition => _player.Position;
        public WaveNumber CurrentWaveNumber => _currentWave.Number;
        public WavePhase CurrentWavePhase => _currentWave.Phase;
        public ArenaBounds ArenaBounds => _arenaBounds;
    }
}

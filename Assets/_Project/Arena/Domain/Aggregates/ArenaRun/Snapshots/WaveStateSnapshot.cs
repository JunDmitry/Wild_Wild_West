using System;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class WaveStateSnapshot
    {
        public WaveStateSnapshot(
            WaveNumber number,
            WavePhase phase,
            int regularEnemiesRemainingToSpawn,
            BossStatus bossStatus)
        {
            if (regularEnemiesRemainingToSpawn < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(regularEnemiesRemainingToSpawn));
            }

            Number = number;
            Phase = phase;
            RegularEnemiesRemainingToSpawn = regularEnemiesRemainingToSpawn;
            BossStatus = bossStatus;
        }

        public WaveNumber Number { get; }

        public WavePhase Phase { get; }

        public int RegularEnemiesRemainingToSpawn { get; }

        public BossStatus BossStatus { get; }
    }
}

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

            Fill(number, phase, regularEnemiesRemainingToSpawn, bossStatus);
        }

        internal WaveStateSnapshot()
        {
        }

        public WaveNumber Number { get; private set; }

        public WavePhase Phase { get; private set; }

        public int RegularEnemiesRemainingToSpawn { get; private set; }

        public BossStatus BossStatus { get; private set; }

        internal void Fill(
            WaveNumber number,
            WavePhase phase,
            int regularEnemiesRemainingToSpawn,
            BossStatus bossStatus)
        {
            Number = number;
            Phase = phase;
            RegularEnemiesRemainingToSpawn = regularEnemiesRemainingToSpawn;
            BossStatus = bossStatus;
        }
    }
}

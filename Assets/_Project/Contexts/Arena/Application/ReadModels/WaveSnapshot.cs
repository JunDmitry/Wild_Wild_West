using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.ReadModels
{
    public sealed class WaveSnapshot
    {
        public WaveSnapshot(
            WaveNumber number,
            WavePhase phase,
            int regularEnemiesRemainingToSpawn,
            BossStatus bossStatus)
        {
            Fill(number, phase, regularEnemiesRemainingToSpawn, bossStatus);
        }

        internal WaveSnapshot()
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

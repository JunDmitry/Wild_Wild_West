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

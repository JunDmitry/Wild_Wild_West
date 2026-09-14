using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    internal sealed class Wave
    {
        public Wave(WaveNumber number)
        {
            Number = number;
            Phase = WavePhase.RegularCombat;
        }

        public WaveNumber Number { get; }
        public WavePhase Phase { get; private set; }
    }
}

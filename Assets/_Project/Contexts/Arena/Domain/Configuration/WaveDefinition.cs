using System;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Configuration
{
    public sealed class WaveDefinition
    {
        public WaveDefinition(
            WaveNumber number,
            int regularEnemyCount)
        {
            if (regularEnemyCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(regularEnemyCount));
            }

            Number = number;
            RegularEnemyCount = regularEnemyCount;

        }

        public WaveNumber Number { get; }
        public int RegularEnemyCount { get; }
    }
}

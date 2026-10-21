using System;
using System.Collections.Generic;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Configuration
{
    public sealed class WaveCatalog
    {
        private readonly WaveDefinition[] _waves;

        public WaveCatalog(IReadOnlyList<WaveDefinition> waves)
        {
            if (waves == null)
            {
                throw new ArgumentNullException(nameof(waves));
            }

            if (waves.Count == 0)
            {
                throw new ArgumentException("At least one wave is required.", nameof(waves));
            }

            WaveDefinition[] copied = new WaveDefinition[waves.Count];
            WaveNumber expected = WaveNumber.First;

            for (int index = 0; index < waves.Count; index++)
            {
                WaveDefinition wave = waves[index];

                if (wave == null)
                {
                    throw new ArgumentException("Waves cannot contain null.", nameof(waves));
                }

                if (wave.Number != expected)
                {
                    throw new ArgumentException("Wave numbers must be sequential from one.", nameof(waves));
                }

                copied[index] = wave;
                expected = expected.Next();
            }

            _waves = copied;
        }

        public WaveNumber Last => _waves[^1].Number;

        public WaveDefinition Get(WaveNumber number)
        {
            int index = number.Value - 1;

            if (index >= _waves.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(number));
            }

            return _waves[index];
        }

        public bool IsLast(WaveNumber number)
        {
            return number == Last;
        }
    }
}

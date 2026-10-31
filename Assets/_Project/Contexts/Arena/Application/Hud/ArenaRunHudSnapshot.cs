using System;
using System.Globalization;

namespace Game.Arena.Application.Hud
{
    public sealed class ArenaRunHudSnapshot
    {
        private int _cachedHealthCurrent;
        private int _cachedHealthMaximum;
        private int _cachedWaveCurrent;
        private int _cachedWaveTotal;
        private int _cachedBossHealthCurrent;
        private int _cachedBossHealthMaximum;
        private bool _cachedBossPresent;
        private bool _hasCachedStrings;

        public ArenaRunHudSnapshot(
            string healthLabel,
            int healthCurrent,
            int healthMaximum,
            string equippedWeaponLabel,
            string equippedWeaponText,
            int waveCurrent,
            int waveTotal,
            bool isDefeated,
            string bossLabel,
            int bossHealthCurrent,
            int bossHealthMaximum,
            bool isBossPresent)
        {
            Fill(
                healthLabel,
                healthCurrent,
                healthMaximum,
                equippedWeaponLabel,
                equippedWeaponText,
                waveCurrent,
                waveTotal,
                isDefeated,
                bossLabel,
                bossHealthCurrent,
                bossHealthMaximum,
                isBossPresent);
        }

        internal ArenaRunHudSnapshot()
        {
        }

        public string HealthLabel { get; private set; }

        public int HealthCurrent { get; private set; }

        public int HealthMaximum { get; private set; }

        public string HealthText { get; private set; }

        public float HealthFraction { get; private set; }

        public string EquippedWeaponLabel { get; private set; }

        public string EquippedWeaponText { get; private set; }

        public int WaveCurrent { get; private set; }

        public int WaveTotal { get; private set; }

        public string WaveText { get; private set; }

        public bool IsDefeated { get; private set; }

        public string BossLabel { get; private set; }

        public int BossHealthCurrent { get; private set; }

        public int BossHealthMaximum { get; private set; }

        public string BossHealthText { get; private set; }

        public float BossHealthFraction { get; private set; }

        public bool IsBossPresent { get; private set; }

        internal void Fill(
            string healthLabel,
            int healthCurrent,
            int healthMaximum,
            string equippedWeaponLabel,
            string equippedWeaponText,
            int waveCurrent,
            int waveTotal,
            bool isDefeated,
            string bossLabel,
            int bossHealthCurrent,
            int bossHealthMaximum,
            bool isBossPresent)
        {
            Validate(
                healthLabel,
                healthCurrent,
                healthMaximum,
                equippedWeaponLabel,
                equippedWeaponText,
                waveCurrent,
                waveTotal,
                bossLabel,
                bossHealthCurrent,
                bossHealthMaximum,
                isBossPresent);

            HealthLabel = healthLabel;
            HealthCurrent = healthCurrent;
            HealthMaximum = healthMaximum;

            if (_hasCachedStrings == false
                || _cachedHealthCurrent != healthCurrent
                || _cachedHealthMaximum != healthMaximum)
            {
                HealthText = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} / {1}",
                    healthCurrent,
                    healthMaximum);
                HealthFraction = (float)healthCurrent / healthMaximum;
                _cachedHealthCurrent = healthCurrent;
                _cachedHealthMaximum = healthMaximum;
            }

            EquippedWeaponLabel = equippedWeaponLabel;
            EquippedWeaponText = equippedWeaponText;
            WaveCurrent = waveCurrent;
            WaveTotal = waveTotal;

            if (_hasCachedStrings == false
                || _cachedWaveCurrent != waveCurrent
                || _cachedWaveTotal != waveTotal)
            {
                WaveText = string.Format(
                    CultureInfo.InvariantCulture,
                    "Wave {0} of {1}",
                    waveCurrent,
                    waveTotal);
                _cachedWaveCurrent = waveCurrent;
                _cachedWaveTotal = waveTotal;
            }

            IsDefeated = isDefeated;
            BossLabel = bossLabel;
            BossHealthCurrent = bossHealthCurrent;
            BossHealthMaximum = bossHealthMaximum;

            if (_hasCachedStrings == false
                || _cachedBossPresent != isBossPresent
                || _cachedBossHealthCurrent != bossHealthCurrent
                || _cachedBossHealthMaximum != bossHealthMaximum)
            {
                BossHealthText = isBossPresent
                    ? string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} / {1}",
                        bossHealthCurrent,
                        bossHealthMaximum)
                    : string.Empty;
                BossHealthFraction = isBossPresent
                    ? (float)bossHealthCurrent / bossHealthMaximum
                    : 0f;
                _cachedBossPresent = isBossPresent;
                _cachedBossHealthCurrent = bossHealthCurrent;
                _cachedBossHealthMaximum = bossHealthMaximum;
                _hasCachedStrings = true;
            }

            IsBossPresent = isBossPresent;
        }

        private static void Validate(
            string healthLabel,
            int healthCurrent,
            int healthMaximum,
            string equippedWeaponLabel,
            string equippedWeaponText,
            int waveCurrent,
            int waveTotal,
            string bossLabel,
            int bossHealthCurrent,
            int bossHealthMaximum,
            bool isBossPresent)
        {
            if (string.IsNullOrWhiteSpace(healthLabel))
            {
                throw new ArgumentException("Health label cannot be empty.", nameof(healthLabel));
            }

            if (healthMaximum <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(healthMaximum));
            }

            if (healthCurrent < 0 || healthCurrent > healthMaximum)
            {
                throw new ArgumentOutOfRangeException(nameof(healthCurrent));
            }

            if (string.IsNullOrWhiteSpace(equippedWeaponLabel))
            {
                throw new ArgumentException("Equipped weapon label cannot be empty.", nameof(equippedWeaponLabel));
            }

            if (string.IsNullOrWhiteSpace(equippedWeaponText))
            {
                throw new ArgumentException("Equipped weapon text cannot be empty.", nameof(equippedWeaponText));
            }

            if (waveCurrent < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(waveCurrent));
            }

            if (waveTotal < waveCurrent)
            {
                throw new ArgumentException("Wave total cannot precede the current wave.", nameof(waveTotal));
            }

            if (string.IsNullOrWhiteSpace(bossLabel))
            {
                throw new ArgumentException("Boss label cannot be empty.", nameof(bossLabel));
            }

            if (isBossPresent)
            {
                if (bossHealthMaximum <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(bossHealthMaximum));
                }

                if (bossHealthCurrent < 0 || bossHealthCurrent > bossHealthMaximum)
                {
                    throw new ArgumentOutOfRangeException(nameof(bossHealthCurrent));
                }
            }
            else if (bossHealthCurrent != 0 || bossHealthMaximum != 0)
            {
                throw new ArgumentException(
                    "Boss health values must be zero when no boss is present.",
                    nameof(bossHealthCurrent));
            }
        }
    }
}

using System;
using System.Globalization;

namespace Game.Arena.Application.Hud
{
    public sealed class ArenaRunHudSnapshot
    {
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

            HealthLabel = healthLabel;
            HealthCurrent = healthCurrent;
            HealthMaximum = healthMaximum;
            HealthText = string.Format(
                CultureInfo.InvariantCulture,
                "{0} / {1}",
                healthCurrent,
                healthMaximum);
            HealthFraction = (float)healthCurrent / healthMaximum;
            EquippedWeaponLabel = equippedWeaponLabel;
            EquippedWeaponText = equippedWeaponText;
            WaveCurrent = waveCurrent;
            WaveTotal = waveTotal;
            WaveText = string.Format(
                CultureInfo.InvariantCulture,
                "Wave {0} of {1}",
                waveCurrent,
                waveTotal);
            IsDefeated = isDefeated;
            BossLabel = bossLabel;
            BossHealthCurrent = bossHealthCurrent;
            BossHealthMaximum = bossHealthMaximum;
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
            IsBossPresent = isBossPresent;
        }

        public string HealthLabel { get; }

        public int HealthCurrent { get; }

        public int HealthMaximum { get; }

        public string HealthText { get; }

        public float HealthFraction { get; }

        public string EquippedWeaponLabel { get; }

        public string EquippedWeaponText { get; }

        public int WaveCurrent { get; }

        public int WaveTotal { get; }

        public string WaveText { get; }

        public bool IsDefeated { get; }

        public string BossLabel { get; }

        public int BossHealthCurrent { get; }

        public int BossHealthMaximum { get; }

        public string BossHealthText { get; }

        public float BossHealthFraction { get; }

        public bool IsBossPresent { get; }
    }
}

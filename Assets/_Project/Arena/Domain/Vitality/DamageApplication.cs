using System;

namespace Game.Arena.Domain.Vitality
{
    public readonly struct DamageApplication : IEquatable<DamageApplication>
    {
        public DamageApplication(
            Health sourceHealth,
            DamageAmount requestedDamage,
            DamageAmount appliedDamage,
            Health remainingHealth)
        {
            if (sourceHealth.IsValid == false)
            {
                throw new ArgumentException("Source Health is invalid.", nameof(sourceHealth));
            }

            if (requestedDamage.Points <= 0)
            {
                throw new ArgumentException("Requested Damage is invalid.", nameof(requestedDamage));
            }

            if (appliedDamage.Points <= 0)
            {
                throw new ArgumentException("Applied Damage is invalid.", nameof(appliedDamage));
            }

            if (appliedDamage > requestedDamage)
            {
                throw new ArgumentException("Applied Damage cannot exceed requested Damage.", nameof(appliedDamage));
            }

            if (remainingHealth.IsValid == false)
            {
                throw new ArgumentException("Remaining Health is invalid.", nameof(remainingHealth));
            }

            int expectedCurrent = sourceHealth.Current - appliedDamage.Points;

            if (expectedCurrent != remainingHealth.Current)
            {
                throw new ArgumentException("Remaining Health does not match Applied Damage.", nameof(remainingHealth));
            }

            SourceHealth = sourceHealth;
            RequestedDamage = requestedDamage;
            AppliedDamage = appliedDamage;
            RemainingHealth = remainingHealth;
        }

        public Health SourceHealth { get; }

        public DamageAmount RequestedDamage { get; }

        public DamageAmount AppliedDamage { get; }

        public Health RemainingHealth { get; }

        public bool IsLethal => RemainingHealth.IsDepleted;

        public bool Equals(DamageApplication other)
        {
            return SourceHealth.Equals(other.SourceHealth)
                && RequestedDamage.Equals(other.RequestedDamage)
                && AppliedDamage.Equals(other.AppliedDamage)
                && RemainingHealth.Equals(other.RemainingHealth);
        }

        public override bool Equals(object obj)
        {
            return obj is DamageApplication other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                SourceHealth,
                RequestedDamage,
                AppliedDamage,
                RemainingHealth);
        }

        public static bool operator ==(DamageApplication left, DamageApplication right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DamageApplication left, DamageApplication right)
        {
            return !left.Equals(right);
        }
    }
}

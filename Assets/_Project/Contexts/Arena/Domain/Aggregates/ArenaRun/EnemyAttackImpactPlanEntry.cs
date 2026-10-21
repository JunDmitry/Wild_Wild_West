using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    internal readonly struct EnemyAttackImpactPlanEntry
    {
        private EnemyAttackImpactPlanEntry(
            EnemyId enemyId,
            AttackId attackId,
            AttackOutcome outcome,
            DamageAmount appliedDamage,
            Health remainingHealth)
        {
            EnemyId = enemyId;
            AttackId = attackId;
            Outcome = outcome;
            AppliedDamage = appliedDamage;
            RemainingHealth = remainingHealth;
        }

        public EnemyId EnemyId { get; }

        public AttackId AttackId { get; }

        public AttackOutcome Outcome { get; }

        public DamageAmount AppliedDamage { get; }

        public Health RemainingHealth { get; }

        public bool IsHit => Outcome == AttackOutcome.Hit;

        public static EnemyAttackImpactPlanEntry Hit(
            EnemyId enemyId,
            AttackId attackId,
            DamageAmount appliedDamage,
            Health remainingHealth)
        {
            return new EnemyAttackImpactPlanEntry(
                enemyId,
                attackId,
                AttackOutcome.Hit,
                appliedDamage,
                remainingHealth);
        }

        public static EnemyAttackImpactPlanEntry Miss(
            EnemyId enemyId,
            AttackId attackId,
            Health currentHealth)
        {
            return new EnemyAttackImpactPlanEntry(
                enemyId,
                attackId,
                AttackOutcome.Miss,
                default,
                currentHealth);
        }
    }
}

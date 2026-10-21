using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Attack
{
    public sealed class PlayerAttackImpactResolution : IInteractionResolution
    {
        private readonly ReadOnlyCollection<EnemyId> _hitEnemies;

        public PlayerAttackImpactResolution(
            InteractionCorrelation correlation,
            IReadOnlyList<EnemyId> hitEnemies)
        {
            if (correlation.ArenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(correlation));
            }

            if (correlation.InteractionId.IsNone)
            {
                throw new ArgumentException("InteractionId cannot be None.", nameof(correlation));
            }

            if (hitEnemies == null)
            {
                throw new ArgumentNullException(nameof(hitEnemies));
            }

            EnemyId[] copied = new EnemyId[hitEnemies.Count];

            for (int index = 0; index < hitEnemies.Count; index++)
            {
                copied[index] = hitEnemies[index];
            }

            Correlation = correlation;
            _hitEnemies = Array.AsReadOnly(copied);
        }

        public InteractionCorrelation Correlation { get; }

        public InteractionKind Kind => InteractionKind.PlayerAttack;

        public IReadOnlyList<EnemyId> HitEnemies => _hitEnemies;

        public static PlayerAttackImpactResolution Miss(InteractionCorrelation correlation)
        {
            return new PlayerAttackImpactResolution(correlation, Array.Empty<EnemyId>());
        }
    }
}

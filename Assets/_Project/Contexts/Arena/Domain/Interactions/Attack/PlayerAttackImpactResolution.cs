using System;
using System.Collections.Generic;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions.Attack
{
    public sealed class PlayerAttackImpactResolution : IInteractionResolution
    {
        private readonly List<EnemyId> _hitEnemiesBuffer;

        public PlayerAttackImpactResolution(
            InteractionCorrelation correlation,
            IReadOnlyList<EnemyId> hitEnemies)
        {
            _hitEnemiesBuffer = new List<EnemyId>(hitEnemies == null ? 0 : hitEnemies.Count);
            HitEnemies = _hitEnemiesBuffer.AsReadOnly();
            Fill(correlation, hitEnemies);
        }

        /// <summary>
        /// Creates an empty shell for the per-tick reuse path (ADR-0035
        /// requirement 9). Fill it before handing it to the aggregate.
        /// </summary>
        public PlayerAttackImpactResolution(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _hitEnemiesBuffer = new List<EnemyId>(capacity);
            HitEnemies = _hitEnemiesBuffer.AsReadOnly();
        }

        public InteractionCorrelation Correlation { get; private set; }

        public InteractionKind Kind => InteractionKind.PlayerAttack;

        public IReadOnlyList<EnemyId> HitEnemies { get; }

        public void Fill(
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

            Correlation = correlation;
            _hitEnemiesBuffer.Clear();

            for (int index = 0; index < hitEnemies.Count; index++)
            {
                _hitEnemiesBuffer.Add(hitEnemies[index]);
            }
        }

        public static PlayerAttackImpactResolution Miss(InteractionCorrelation correlation)
        {
            return new PlayerAttackImpactResolution(correlation, Array.Empty<EnemyId>());
        }
    }
}

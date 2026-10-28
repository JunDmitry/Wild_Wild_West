using System;
using System.Collections.Generic;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Events;

namespace Game.Arena.Application.Notifications
{
    /// <summary>
    /// Per-slot pool of Application notifications. The tick frame buffer owns
    /// one of these per published slot, so a notification is reused only when
    /// its slot is next published — the two-tick rule of ADR-0035 point 2.
    /// </summary>
    internal sealed class ArenaRunNotificationPool
    {
        private ArenaRunVictoryNotification[] _victories;
        private ArenaRunEnemyDamagedNotification[] _damaged;
        private ArenaRunEnemyAttackResolvedNotification[] _resolved;

        private int _victoryCount;
        private int _damagedCount;
        private int _resolvedCount;

        public ArenaRunNotificationPool(int enemyCapacity)
        {
            if (enemyCapacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(enemyCapacity));
            }

            _victories = new ArenaRunVictoryNotification[1];
            _damaged = new ArenaRunEnemyDamagedNotification[enemyCapacity];
            _resolved = new ArenaRunEnemyAttackResolvedNotification[enemyCapacity];
        }

        public void Reset()
        {
            _victoryCount = 0;
            _damagedCount = 0;
            _resolvedCount = 0;
        }

        public void Fill(
            IReadOnlyList<IArenaDomainEvent> domainEvents,
            List<IArenaRunNotification> target)
        {
            if (domainEvents == null)
            {
                throw new ArgumentNullException(nameof(domainEvents));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Reset();
            target.Clear();

            for (int index = 0; index < domainEvents.Count; index++)
            {
                IArenaDomainEvent domainEvent = domainEvents[index];

                if (domainEvent == null)
                {
                    throw new ArgumentException(
                        "Domain events cannot contain null.",
                        nameof(domainEvents));
                }

                if (domainEvent is ArenaRunVictorious victorious)
                {
                    ArenaRunVictoryNotification notification = TakeVictory();
                    notification.Fill(victorious.ArenaRunId, victorious.AggregateRevision);
                    target.Add(notification);
                }
                else if (domainEvent is EnemyDamaged damaged)
                {
                    ArenaRunEnemyDamagedNotification notification = TakeDamaged();
                    notification.Fill(
                        damaged.ArenaRunId,
                        damaged.AggregateRevision,
                        damaged.EnemyId);
                    target.Add(notification);
                }
                else if (domainEvent is EnemyAttackCompleted completed)
                {
                    ArenaRunEnemyAttackResolvedNotification notification = TakeResolved();
                    notification.Fill(
                        completed.ArenaRunId,
                        completed.AggregateRevision,
                        completed.EnemyId,
                        completed.Outcome == AttackOutcome.Hit
                            ? ArenaRunEnemyAttackOutcome.Hit
                            : ArenaRunEnemyAttackOutcome.Miss);
                    target.Add(notification);
                }
                else if (domainEvent is EnemyAttackCancelled cancelled)
                {
                    ArenaRunEnemyAttackResolvedNotification notification = TakeResolved();
                    notification.Fill(
                        cancelled.ArenaRunId,
                        cancelled.AggregateRevision,
                        cancelled.EnemyId,
                        ArenaRunEnemyAttackOutcome.Cancelled);
                    target.Add(notification);
                }
            }
        }

        private ArenaRunVictoryNotification TakeVictory()
        {
            if (_victoryCount == _victories.Length)
            {
                Array.Resize(ref _victories, _victories.Length * 2);
            }

            ArenaRunVictoryNotification notification = _victories[_victoryCount];

            if (notification == null)
            {
                notification = new ArenaRunVictoryNotification();
                _victories[_victoryCount] = notification;
            }

            _victoryCount++;

            return notification;
        }

        private ArenaRunEnemyDamagedNotification TakeDamaged()
        {
            if (_damagedCount == _damaged.Length)
            {
                Array.Resize(ref _damaged, _damaged.Length * 2);
            }

            ArenaRunEnemyDamagedNotification notification = _damaged[_damagedCount];

            if (notification == null)
            {
                notification = new ArenaRunEnemyDamagedNotification();
                _damaged[_damagedCount] = notification;
            }

            _damagedCount++;

            return notification;
        }

        private ArenaRunEnemyAttackResolvedNotification TakeResolved()
        {
            if (_resolvedCount == _resolved.Length)
            {
                Array.Resize(ref _resolved, _resolved.Length * 2);
            }

            ArenaRunEnemyAttackResolvedNotification notification = _resolved[_resolvedCount];

            if (notification == null)
            {
                notification = new ArenaRunEnemyAttackResolvedNotification();
                _resolved[_resolvedCount] = notification;
            }

            _resolvedCount++;

            return notification;
        }
    }
}

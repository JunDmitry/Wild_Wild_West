using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Events
{
    /// <summary>
    /// The per-run Domain event pool. Two slots alternate: an event filled in
    /// slot A stays untouched for the following tick (which fills slot B) and
    /// is released when slot A is reused two ticks later (ADR-0036 point 4).
    /// Capacity is derived from the largest catalogue wave; a bucket grows on
    /// demand only if a tick produces more events of one type than expected.
    /// </summary>
    internal sealed class ArenaDomainEventPool
    {
        private const int SingletonCapacity = 1;

        private readonly EventBucket<ArenaRunDefeated> _arenaRunDefeated;
        private readonly EventBucket<ArenaRunVictorious> _arenaRunVictorious;
        private readonly EventBucket<EnemyAttackCancelled> _enemyAttackCancelled;
        private readonly EventBucket<EnemyAttackCompleted> _enemyAttackCompleted;
        private readonly EventBucket<EnemyAttackStarted> _enemyAttackStarted;
        private readonly EventBucket<EnemyDamaged> _enemyDamaged;
        private readonly EventBucket<EnemyDefeated> _enemyDefeated;
        private readonly EventBucket<EnemySpawned> _enemySpawned;
        private readonly EventBucket<PlayerAttackCancelled> _playerAttackCancelled;
        private readonly EventBucket<PlayerAttackCompleted> _playerAttackCompleted;
        private readonly EventBucket<PlayerAttackStarted> _playerAttackStarted;
        private readonly EventBucket<PlayerDamaged> _playerDamaged;
        private readonly EventBucket<PlayerDefeated> _playerDefeated;
        private readonly EventBucket<WaveCompleted> _waveCompleted;
        private readonly EventBucket<WavePhaseChanged> _wavePhaseChanged;
        private readonly EventBucket<WaveStarted> _waveStarted;

        public ArenaDomainEventPool(int enemyCapacity)
        {
            if (enemyCapacity < SingletonCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(enemyCapacity));
            }

            _arenaRunDefeated = new EventBucket<ArenaRunDefeated>(SingletonCapacity, () => new ArenaRunDefeated());
            _arenaRunVictorious = new EventBucket<ArenaRunVictorious>(SingletonCapacity, () => new ArenaRunVictorious());
            _enemyAttackCancelled = new EventBucket<EnemyAttackCancelled>(enemyCapacity, () => new EnemyAttackCancelled());
            _enemyAttackCompleted = new EventBucket<EnemyAttackCompleted>(enemyCapacity, () => new EnemyAttackCompleted());
            _enemyAttackStarted = new EventBucket<EnemyAttackStarted>(enemyCapacity, () => new EnemyAttackStarted());
            _enemyDamaged = new EventBucket<EnemyDamaged>(enemyCapacity, () => new EnemyDamaged());
            _enemyDefeated = new EventBucket<EnemyDefeated>(enemyCapacity, () => new EnemyDefeated());
            _enemySpawned = new EventBucket<EnemySpawned>(enemyCapacity, () => new EnemySpawned());
            _playerAttackCancelled = new EventBucket<PlayerAttackCancelled>(SingletonCapacity, () => new PlayerAttackCancelled());
            _playerAttackCompleted = new EventBucket<PlayerAttackCompleted>(SingletonCapacity, () => new PlayerAttackCompleted());
            _playerAttackStarted = new EventBucket<PlayerAttackStarted>(SingletonCapacity, () => new PlayerAttackStarted());
            _playerDamaged = new EventBucket<PlayerDamaged>(enemyCapacity, () => new PlayerDamaged());
            _playerDefeated = new EventBucket<PlayerDefeated>(SingletonCapacity, () => new PlayerDefeated());
            _waveCompleted = new EventBucket<WaveCompleted>(SingletonCapacity, () => new WaveCompleted());
            _wavePhaseChanged = new EventBucket<WavePhaseChanged>(SingletonCapacity, () => new WavePhaseChanged());
            _waveStarted = new EventBucket<WaveStarted>(SingletonCapacity, () => new WaveStarted());
        }

        public void BeginTick()
        {
            _arenaRunDefeated.BeginTick();
            _arenaRunVictorious.BeginTick();
            _enemyAttackCancelled.BeginTick();
            _enemyAttackCompleted.BeginTick();
            _enemyAttackStarted.BeginTick();
            _enemyDamaged.BeginTick();
            _enemyDefeated.BeginTick();
            _enemySpawned.BeginTick();
            _playerAttackCancelled.BeginTick();
            _playerAttackCompleted.BeginTick();
            _playerAttackStarted.BeginTick();
            _playerDamaged.BeginTick();
            _playerDefeated.BeginTick();
            _waveCompleted.BeginTick();
            _wavePhaseChanged.BeginTick();
            _waveStarted.BeginTick();
        }

        public ArenaRunDefeated TakeArenaRunDefeated(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision)
        {
            ArenaRunDefeated domainEvent = _arenaRunDefeated.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision);

            return domainEvent;
        }

        public ArenaRunVictorious TakeArenaRunVictorious(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision)
        {
            ArenaRunVictorious domainEvent = _arenaRunVictorious.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision);

            return domainEvent;
        }

        public EnemyAttackCancelled TakeEnemyAttackCancelled(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            AttackId attackId,
            AttackCancellationCause cause)
        {
            EnemyAttackCancelled domainEvent = _enemyAttackCancelled.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, enemyId, attackId, cause);

            return domainEvent;
        }

        public EnemyAttackCompleted TakeEnemyAttackCompleted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            AttackId attackId,
            AttackOutcome outcome,
            GameTimePoint completedAt)
        {
            EnemyAttackCompleted domainEvent = _enemyAttackCompleted.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, enemyId, attackId, outcome, completedAt);

            return domainEvent;
        }

        public EnemyAttackStarted TakeEnemyAttackStarted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            AttackId attackId,
            GameTimePoint startedAt,
            GameTimePoint impactAt)
        {
            EnemyAttackStarted domainEvent = _enemyAttackStarted.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, enemyId, attackId, startedAt, impactAt);

            return domainEvent;
        }

        public EnemyDamaged TakeEnemyDamaged(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            DamageAmount damage,
            Health remainingHealth)
        {
            EnemyDamaged domainEvent = _enemyDamaged.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, enemyId, damage, remainingHealth);

            return domainEvent;
        }

        public EnemyDefeated TakeEnemyDefeated(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            EnemyKind enemyKind)
        {
            EnemyDefeated domainEvent = _enemyDefeated.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, enemyId, enemyKind);

            return domainEvent;
        }

        public EnemySpawned TakeEnemySpawned(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            EnemyId enemyId,
            EnemyKind enemyKind,
            Position3D position)
        {
            EnemySpawned domainEvent = _enemySpawned.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, enemyId, enemyKind, position);

            return domainEvent;
        }

        public PlayerAttackCancelled TakePlayerAttackCancelled(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId,
            AttackId attackId,
            AttackCancellationCause cause)
        {
            PlayerAttackCancelled domainEvent = _playerAttackCancelled.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, playerId, attackId, cause);

            return domainEvent;
        }

        public PlayerAttackCompleted TakePlayerAttackCompleted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId,
            AttackId attackId,
            WeaponKind weaponKind,
            AttackOutcome outcome,
            GameTimePoint completedAt)
        {
            PlayerAttackCompleted domainEvent = _playerAttackCompleted.Take();
            domainEvent.Fill(
                arenaRunId,
                aggregateRevision,
                playerId,
                attackId,
                weaponKind,
                outcome,
                completedAt);

            return domainEvent;
        }

        public PlayerAttackStarted TakePlayerAttackStarted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId,
            AttackId attackId,
            WeaponKind weaponKind,
            GameTimePoint startedAt,
            GameTimePoint impactAt)
        {
            PlayerAttackStarted domainEvent = _playerAttackStarted.Take();
            domainEvent.Fill(
                arenaRunId,
                aggregateRevision,
                playerId,
                attackId,
                weaponKind,
                startedAt,
                impactAt);

            return domainEvent;
        }

        public PlayerDamaged TakePlayerDamaged(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId,
            EnemyId sourceEnemyId,
            AttackId sourceAttackId,
            DamageAmount appliedDamage,
            Health remainingHealth)
        {
            PlayerDamaged domainEvent = _playerDamaged.Take();
            domainEvent.Fill(
                arenaRunId,
                aggregateRevision,
                playerId,
                sourceEnemyId,
                sourceAttackId,
                appliedDamage,
                remainingHealth);

            return domainEvent;
        }

        public PlayerDefeated TakePlayerDefeated(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            PlayerId playerId)
        {
            PlayerDefeated domainEvent = _playerDefeated.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, playerId);

            return domainEvent;
        }

        public WaveCompleted TakeWaveCompleted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            WaveNumber waveNumber)
        {
            WaveCompleted domainEvent = _waveCompleted.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, waveNumber);

            return domainEvent;
        }

        public WavePhaseChanged TakeWavePhaseChanged(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            WaveNumber waveNumber,
            WavePhase previousPhase,
            WavePhase newPhase)
        {
            WavePhaseChanged domainEvent = _wavePhaseChanged.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, waveNumber, previousPhase, newPhase);

            return domainEvent;
        }

        public WaveStarted TakeWaveStarted(
            ArenaRunId arenaRunId,
            AggregateRevision aggregateRevision,
            WaveNumber waveNumber,
            WavePhase initialPhase)
        {
            WaveStarted domainEvent = _waveStarted.Take();
            domainEvent.Fill(arenaRunId, aggregateRevision, waveNumber, initialPhase);

            return domainEvent;
        }

        private sealed class EventBucket<T>
            where T : class, IPooledArenaDomainEvent
        {
            private readonly Func<T> _create;
            private readonly T[][] _slots;
            private readonly int[] _counts;
            private int _active;

            public EventBucket(int capacity, Func<T> create)
            {
                _create = create ?? throw new ArgumentNullException(nameof(create));
                _slots = new T[2][];
                _slots[0] = new T[capacity];
                _slots[1] = new T[capacity];
                _counts = new int[2];
                _active = 0;
            }

            public void BeginTick()
            {
                _active ^= 1;
                Release(_active);
            }

            public T Take()
            {
                int slot = _active;
                int index = _counts[slot];

                if (index == _slots[slot].Length)
                {
                    Grow(slot);
                }

                T item = _slots[slot][index];

                if (item == null)
                {
                    item = _create();
                    _slots[slot][index] = item;
                }

                _counts[slot] = index + 1;

                return item;
            }

            private void Release(int slot)
            {
                T[] items = _slots[slot];
                int count = _counts[slot];

                for (int index = 0; index < count; index++)
                {
                    items[index].Reset();
                }

                _counts[slot] = 0;
            }

            private void Grow(int slot)
            {
                T[] current = _slots[slot];
                T[] grown = new T[current.Length * 2];

                Array.Copy(current, grown, current.Length);
                _slots[slot] = grown;
            }
        }
    }
}

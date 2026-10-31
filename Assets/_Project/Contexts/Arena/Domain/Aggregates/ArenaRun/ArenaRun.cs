using System;
using System.Collections.Generic;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Attack;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRun
    {
        private static readonly IReadOnlyList<IArenaDomainEvent> s_noEvents = Array.Empty<IArenaDomainEvent>();
        private static readonly Comparison<EnemyId> s_compareEnemyIdValues = CompareEnemyIds;

        private readonly ArenaDefinition _arenaDefinition;
        private readonly EnemyCatalog _enemyCatalog;
        private readonly WaveCatalog _waveCatalog;
        private readonly WeaponCatalog _weaponCatalog;
        private readonly MovementPathPolicy _movementPathPolicy;

        private readonly Player _player;
        private readonly Dictionary<EnemyId, Enemy> _enemies;
        private readonly PendingInteractionLedger _interactionLedger;

        private Wave _currentWave;
        private PlayerMovementRequest _pendingMovementRequest;
        private EnemySpawnRequest _pendingSpawnRequest;
        private PlayerAttackImpactRequest _pendingAttackImpactRequest;
        private EnemyMovementBatchRequest _pendingEnemyMovementBatchRequest;
        private AttackId _lastAttackId;

        private readonly ArenaDomainEventPool _eventPool;
        private readonly List<IArenaDomainEvent> _operationEvents;
        private readonly List<EnemyId> _orderedEnemyIds;
        private readonly List<EnemyId> _scratchTargets;
        private readonly List<EnemyId> _scratchDefeated;
        private readonly List<EnemyId> _scratchEligible;
        private readonly List<PendingEnemyAttack> _scratchStartedAttacks;
        private readonly List<EnemyAttackImpactPlanEntry> _scratchImpactPlan;
        private readonly List<EnemyMovementIntent> _scratchMovementIntents;
        private readonly Dictionary<EnemyId, Position3D> _scratchAcceptedPositions;
        private readonly HashSet<EnemyId> _scratchExpectedIds;
        private readonly ArenaRunChange _change;
        private readonly EnemyMovementBatchRequest _movementBatchRequest;

        private readonly PlayerMovementRequestOutcome _movementRequestOutcome;
        private readonly PlayerMovementResolutionOutcome _movementResolutionOutcome;
        private readonly WeaponSwitchOutcome _weaponSwitchOutcome;
        private readonly TimeAdvanceOutcome _timeAdvanceOutcome;
        private readonly EnemySpawnRequestOutcome _spawnRequestOutcome;
        private readonly EnemySpawnResolutionOutcome _spawnResolutionOutcome;
        private readonly PlayerAttackStartOutcome _playerAttackStartOutcome;
        private readonly PlayerAttackImpactRequestOutcome _playerAttackImpactRequestOutcome;
        private readonly PlayerAttackImpactOutcome _playerAttackImpactOutcome;
        private readonly EnemyAttackStartOutcome _enemyAttackStartOutcome;
        private readonly EnemyAttackImpactOutcome _enemyAttackImpactOutcome;
        private readonly EnemyMovementBatchRequestOutcome _enemyMovementBatchRequestOutcome;
        private readonly EnemyMovementBatchResolutionOutcome _enemyMovementBatchResolutionOutcome;
        private readonly InteractionCancellationOutcome _cancellationOutcome;

        internal ArenaRun(
            ArenaRunId id,
            Player runPlayer,
            Wave runWave,
            ArenaDefinition runArenaDefinition,
            WeaponCatalog runWeaponCatalog,
            EnemyCatalog runEnemyCatalog,
            WaveCatalog runWaveCatalog,
            MovementPathPolicy runMovementPathPolicy)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(id));
            }

            if (runPlayer == null)
            {
                throw new ArgumentNullException(nameof(runPlayer));
            }

            if (runWave == null)
            {
                throw new ArgumentNullException(nameof(runWave));
            }

            if (runArenaDefinition == null)
            {
                throw new ArgumentNullException(nameof(runArenaDefinition));
            }

            if (runWeaponCatalog == null)
            {
                throw new ArgumentNullException(nameof(runWeaponCatalog));
            }

            if (runEnemyCatalog == null)
            {
                throw new ArgumentNullException(nameof(runEnemyCatalog));
            }

            if (runWaveCatalog == null)
            {
                throw new ArgumentNullException(nameof(runWaveCatalog));
            }

            if (runMovementPathPolicy == null)
            {
                throw new ArgumentNullException(nameof(runMovementPathPolicy));
            }

            ArenaBounds bounds = runArenaDefinition.Bounds;

            if (bounds.CanContain(runPlayer.CollisionRadius) == false)
            {
                throw new ArgumentException("Arena cannot contain the Player.", nameof(runArenaDefinition));
            }

            if (bounds.Contains(runPlayer.Position, runPlayer.CollisionRadius) == false)
            {
                throw new ArgumentException("Player position is outside the Arena.", nameof(runPlayer));
            }

            Id = id;
            Status = ArenaRunStatus.Playing;
            Revision = AggregateRevision.Initial;
            _lastAttackId = AttackId.None;
            _interactionLedger = new PendingInteractionLedger(id);
            _enemies = new Dictionary<EnemyId, Enemy>();
            _player = runPlayer;
            _currentWave = runWave;
            _arenaDefinition = runArenaDefinition;
            _weaponCatalog = runWeaponCatalog;
            _enemyCatalog = runEnemyCatalog;
            _waveCatalog = runWaveCatalog;
            _movementPathPolicy = runMovementPathPolicy;

            CurrentTime = new GameTimePoint(0);

            EnemyCapacity = CalculateEnemyCapacity(runWaveCatalog);
            _eventPool = new ArenaDomainEventPool(EnemyCapacity);
            _operationEvents = new List<IArenaDomainEvent>(EnemyCapacity * 4);
            _orderedEnemyIds = new List<EnemyId>(EnemyCapacity);
            _scratchTargets = new List<EnemyId>(EnemyCapacity);
            _scratchDefeated = new List<EnemyId>(EnemyCapacity);
            _scratchEligible = new List<EnemyId>(EnemyCapacity);
            _scratchStartedAttacks = new List<PendingEnemyAttack>(EnemyCapacity);
            _scratchImpactPlan = new List<EnemyAttackImpactPlanEntry>(EnemyCapacity);
            _scratchMovementIntents = new List<EnemyMovementIntent>(EnemyCapacity);
            _scratchAcceptedPositions = new Dictionary<EnemyId, Position3D>(EnemyCapacity);
            _scratchExpectedIds = new HashSet<EnemyId>();
            _change = new ArenaRunChange();
            _movementBatchRequest = new EnemyMovementBatchRequest();

            _movementRequestOutcome = new PlayerMovementRequestOutcome();
            _movementResolutionOutcome = new PlayerMovementResolutionOutcome();
            _weaponSwitchOutcome = new WeaponSwitchOutcome();
            _timeAdvanceOutcome = new TimeAdvanceOutcome();
            _spawnRequestOutcome = new EnemySpawnRequestOutcome();
            _spawnResolutionOutcome = new EnemySpawnResolutionOutcome();
            _playerAttackStartOutcome = new PlayerAttackStartOutcome();
            _playerAttackImpactRequestOutcome = new PlayerAttackImpactRequestOutcome();
            _playerAttackImpactOutcome = new PlayerAttackImpactOutcome();
            _enemyAttackStartOutcome = new EnemyAttackStartOutcome();
            _enemyAttackImpactOutcome = new EnemyAttackImpactOutcome();
            _enemyMovementBatchRequestOutcome = new EnemyMovementBatchRequestOutcome();
            _enemyMovementBatchResolutionOutcome = new EnemyMovementBatchResolutionOutcome();
            _cancellationOutcome = new InteractionCancellationOutcome();
        }

        public ArenaRunId Id { get; }
        public ArenaRunStatus Status { get; private set; }
        public AggregateRevision Revision { get; private set; }
        public GameTimePoint CurrentTime { get; private set; }

        public bool HasPendingInteraction => _interactionLedger.HasPending;
        public PlayerId PlayerId => _player.Id;
        public Position3D PlayerPosition => _player.Position;
        public Health PlayerHealth => _player.Health;
        public WeaponKind SelectedWeapon => _player.SelectedWeapon;
        public bool IsSelectedWeaponReady => _player.IsWeaponReady(_player.SelectedWeapon, CurrentTime);
        public bool HasPendingPlayerAttack => _player.HasPendingAttack;
        public WaveNumber CurrentWaveNumber => _currentWave.Number;
        public WavePhase CurrentWavePhase => _currentWave.Phase;
        public ArenaBounds ArenaBounds => _arenaDefinition.Bounds;
        public BossStatus CurrentBossStatus => _currentWave.BossStatus;

        public int RegularEnemiesRemainingToSpawn => _currentWave.RegularEnemiesRemainingToSpawn;
        public int ActiveEnemyCount => _enemies.Count;

        /// <summary>
        /// Upper bound on enemies alive at once: the largest catalogue wave's
        /// regular count plus its boss. Derived, never a literal (ADR-0035).
        /// </summary>
        public int EnemyCapacity { get; }

        /// <summary>
        /// Advances the Domain event pool to the other slot and releases the
        /// events that slot held two ticks ago (ADR-0036 point 3).
        /// </summary>
        public void BeginTick()
        {
            _eventPool.BeginTick();
        }

        public int PendingEnemyAttackCount
        {
            get
            {
                int count = 0;

                foreach (Enemy enemy in _enemies.Values)
                {
                    if (enemy.HasPendingAttack)
                    {
                        count++;
                    }
                }

                return count;
            }
        }
        public int DueEnemyAttackCount
        {
            get
            {
                int count = 0;

                foreach (Enemy enemy in _enemies.Values)
                {
                    if (enemy.HasPendingAttack == false)
                    {
                        continue;
                    }

                    if (enemy.PendingAttack.IsReadyToImpact(CurrentTime))
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public bool ContainsEnemy(EnemyId enemyId)
        {
            return _enemies.ContainsKey(enemyId);
        }

        public bool TryGetEnemyPosition(EnemyId enemyId, out Position3D position)
        {
            if (_enemies.TryGetValue(enemyId, out Enemy enemy))
            {
                position = enemy.Position;
                return true;
            }

            position = default;

            return false;
        }

        public PlayerMovementRequestOutcome RequestPlayerMovement(MovementInput movementInput, GameDuration duration)
        {
            if (Status != ArenaRunStatus.Playing)
            {
                return PlayerMovementRequestOutcome.NotPlaying;
            }

            if (_interactionLedger.HasPending)
            {
                throw new InvalidOperationException("An interaction is already pending.");
            }

            if (movementInput.IsZero)
            {
                return PlayerMovementRequestOutcome.NoMovement;
            }

            if (duration.Seconds <= 0d)
            {
                return PlayerMovementRequestOutcome.NoMovement;
            }

            bool hasDirection = movementInput.TryGetDirection(out Direction3D direction);

            if (hasDirection == false)
            {
                return PlayerMovementRequestOutcome.NoMovement;
            }

            Distance requestedDistance = _player.MovementSpeed
                .DistanceOver(duration)
                .Scaled(movementInput.Magnitude);

            BoundedMovement permittedMovement = _arenaDefinition.Bounds.PermittedMovement(_player.Position, direction, _player.CollisionRadius, requestedDistance);

            if (permittedMovement.IsZero)
            {
                return PlayerMovementRequestOutcome.PositionUnchanged;
            }

            PlanarMovementIntent intent = new(_player.Position, permittedMovement.Direction, permittedMovement.Distance);
            InteractionCorrelation correlation = _interactionLedger.Open(InteractionKind.PlayerMovement, Revision);
            _pendingMovementRequest = new(correlation, intent, _player.CollisionRadius);
            _movementRequestOutcome.Fill(
                PlayerMovementRequestStatus.Requested,
                _pendingMovementRequest);

            return _movementRequestOutcome;
        }

        public WeaponSwitchOutcome SwitchWeapon()
        {
            if (Status != ArenaRunStatus.Playing)
            {
                _weaponSwitchOutcome.Fill(
                    WeaponSwitchStatus.RunIsNotPlaying,
                    SelectedWeapon,
                    CreateNoChange());

                return _weaponSwitchOutcome;
            }

            if (_interactionLedger.HasPending)
            {
                _weaponSwitchOutcome.Fill(
                    WeaponSwitchStatus.InteractionPending,
                    SelectedWeapon,
                    CreateNoChange());

                return _weaponSwitchOutcome;
            }

            if (_player.HasPendingAttack)
            {
                _weaponSwitchOutcome.Fill(
                    WeaponSwitchStatus.AttackPending,
                    SelectedWeapon,
                    CreateNoChange());

                return _weaponSwitchOutcome;
            }

            _player.SwitchWeapon();
            Revision = Revision.Next();

            _weaponSwitchOutcome.Fill(
                WeaponSwitchStatus.Switched,
                SelectedWeapon,
                CreateStateChange());

            return _weaponSwitchOutcome;
        }

        public TimeAdvanceOutcome AdvanceTime(GameDuration duration)
        {
            if (duration.Seconds <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            if (Status != ArenaRunStatus.Playing)
            {
                _timeAdvanceOutcome.Fill(
                    TimeAdvanceStatus.RunIsNotPlaying,
                    CurrentTime,
                    CreateNoChange());

                return _timeAdvanceOutcome;
            }

            if (_interactionLedger.HasPending)
            {
                _timeAdvanceOutcome.Fill(
                    TimeAdvanceStatus.InteractionPending,
                    CurrentTime,
                    CreateNoChange());

                return _timeAdvanceOutcome;
            }

            CurrentTime += duration;
            Revision = Revision.Next();

            _timeAdvanceOutcome.Fill(
                TimeAdvanceStatus.Advanced,
                CurrentTime,
                CreateStateChange());

            return _timeAdvanceOutcome;
        }

        public PlayerMovementResolutionOutcome ApplyPlayerMovement(PlayerMovementResolution resolution)
        {
            InteractionAdmission admission = _interactionLedger.Admit(resolution, Revision);

            if (admission.IsAdmitted == false)
            {
                _movementResolutionOutcome.Fill(
                    PlayerMovementResolutionStatus.Rejected,
                    MapRejectionReason(admission.RejectionReason),
                    CreateNoChange());

                return _movementResolutionOutcome;
            }

            PlayerMovementResolutionRejectionReason geometryRejection = ValidateAcceptedPosition(_pendingMovementRequest, resolution.AcceptedPosition);

            if (geometryRejection != PlayerMovementResolutionRejectionReason.None)
            {
                _movementResolutionOutcome.Fill(
                    PlayerMovementResolutionStatus.Rejected,
                    geometryRejection,
                    CreateNoChange());

                return _movementResolutionOutcome;
            }

            _interactionLedger.Complete(resolution.Correlation.InteractionId);

            if (_player.Position == resolution.AcceptedPosition)
            {
                _movementResolutionOutcome.Fill(
                    PlayerMovementResolutionStatus.AcceptedWithoutStateChange,
                    PlayerMovementResolutionRejectionReason.None,
                    CreateNoChange());

                return _movementResolutionOutcome;
            }

            _player.MoveTo(resolution.AcceptedPosition);
            Revision = Revision.Next();

            _movementResolutionOutcome.Fill(
                PlayerMovementResolutionStatus.Applied,
                PlayerMovementResolutionRejectionReason.None,
                CreateStateChange());

            return _movementResolutionOutcome;
        }

        public EnemySpawnRequestOutcome RequestEnemySpawn()
        {
            if (Status != ArenaRunStatus.Playing)
            {
                return EnemySpawnRequestOutcome.RunIsNotPlaying;
            }

            if (_interactionLedger.HasPending)
            {
                throw new InvalidOperationException("An interaction is already pending.");
            }

            EnemyKind kind;

            if (_currentWave.IsRegularSpawnDue)
            {
                kind = EnemyKind.Regular;
            }
            else if (_currentWave.IsBossSpawnDue)
            {
                kind = EnemyKind.Boss;
            }
            else
            {
                return EnemySpawnRequestOutcome.NoSpawnDue;
            }

            InteractionCorrelation correlation = _interactionLedger.Open(InteractionKind.EnemySpawn, Revision);

            _pendingSpawnRequest = new EnemySpawnRequest(
                correlation,
                kind,
                _enemyCatalog.Get(kind).CollisionRadius);

            _spawnRequestOutcome.Fill(
                EnemySpawnRequestStatus.Requested,
                _pendingSpawnRequest);

            return _spawnRequestOutcome;
        }

        public EnemySpawnResolutionOutcome ApplyEnemySpawn(EnemySpawnResolution resolution)
        {
            InteractionAdmission admission = _interactionLedger.Admit(resolution, Revision);

            if (admission.IsAdmitted == false)
            {
                return FillSpawnRejection(MapSpawnRejectionReason(admission.RejectionReason));
            }

            if (resolution.EnemyId.IsNone)
            {
                return FillSpawnRejection(EnemySpawnResolutionRejectionReason.EnemyIdIsNone);
            }

            if (_enemies.ContainsKey(resolution.EnemyId))
            {
                return FillSpawnRejection(EnemySpawnResolutionRejectionReason.DuplicateEnemyId);
            }

            if (_arenaDefinition.IsSpawnPosition(resolution.SpawnPosition) == false)
            {
                return FillSpawnRejection(EnemySpawnResolutionRejectionReason.SpawnPositionOutsidePerimeter);
            }

            _interactionLedger.Complete(resolution.Correlation.InteractionId);

            EnemyKind kind = _pendingSpawnRequest.EnemyKind;
            Enemy enemy = new(resolution.EnemyId, _enemyCatalog.Get(kind), resolution.SpawnPosition);
            _enemies.Add(enemy.Id, enemy);

            if (kind == EnemyKind.Regular)
            {
                _currentWave.RecordRegularSpawn();
            }
            else
            {
                _currentWave.RecordBossSpawn();
            }

            Revision = Revision.Next();

            _operationEvents.Clear();
            _operationEvents.Add(_eventPool.TakeEnemySpawned(
                Id,
                Revision,
                enemy.Id,
                enemy.Kind,
                enemy.Position));

            _spawnResolutionOutcome.Fill(
                EnemySpawnResolutionStatus.Spawned,
                EnemySpawnResolutionRejectionReason.None,
                CreateChange(true));

            return _spawnResolutionOutcome;
        }

        private EnemySpawnResolutionOutcome FillSpawnRejection(
            EnemySpawnResolutionRejectionReason reason)
        {
            _spawnResolutionOutcome.Fill(
                EnemySpawnResolutionStatus.Rejected,
                reason,
                CreateNoChange());

            return _spawnResolutionOutcome;
        }

        public PlayerAttackStartOutcome StartPlayerAttack()
        {
            if (Status != ArenaRunStatus.Playing)
            {
                return FillAttackStart(PlayerAttackStartStatus.RunIsNotPlaying, default);
            }

            if (_interactionLedger.HasPending)
            {
                return FillAttackStart(PlayerAttackStartStatus.InteractionPending, default);
            }

            if (_player.HasPendingAttack)
            {
                return FillAttackStart(PlayerAttackStartStatus.AttackAlreadyPending, default);
            }

            if (_player.IsWeaponReady(SelectedWeapon, CurrentTime) == false)
            {
                return FillAttackStart(PlayerAttackStartStatus.WeaponNotReady, default);
            }

            WeaponDefinition weapon = _weaponCatalog.Get(SelectedWeapon);
            AttackId nextAttackId = _lastAttackId.Next();
            PendingPlayerAttack pendingAttack = _player.StartAttack(nextAttackId, weapon, CurrentTime);

            _lastAttackId = nextAttackId;
            Revision = Revision.Next();

            _operationEvents.Clear();
            _operationEvents.Add(_eventPool.TakePlayerAttackStarted(
                Id,
                Revision,
                _player.Id,
                pendingAttack.Id,
                pendingAttack.WeaponKind,
                pendingAttack.StartedAt,
                pendingAttack.ImpactAt));

            return FillAttackStart(PlayerAttackStartStatus.Started, pendingAttack);
        }

        private PlayerAttackStartOutcome FillAttackStart(
            PlayerAttackStartStatus status,
            PendingPlayerAttack attack)
        {
            _playerAttackStartOutcome.Fill(
                status,
                attack,
                status == PlayerAttackStartStatus.Started
                    ? CreateChange(true)
                    : CreateNoChange());

            return _playerAttackStartOutcome;
        }

        public PlayerAttackImpactRequestOutcome RequestPlayerAttackImpact(Direction3D aimDirection)
        {
            if (aimDirection.IsValid == false)
            {
                throw new ArgumentException("Direction is invalid.", nameof(aimDirection));
            }

            if (Status != ArenaRunStatus.Playing)
            {
                return PlayerAttackImpactRequestOutcome.RunIsNotPlaying;
            }

            if (_interactionLedger.HasPending)
            {
                throw new InvalidOperationException("An interaction is already pending.");
            }

            if (_player.HasPendingAttack == false)
            {
                return PlayerAttackImpactRequestOutcome.NoPendingAttack;
            }

            PendingPlayerAttack attack = _player.PendingAttack;

            if (attack.IsReadyToImpact(CurrentTime) == false)
            {
                return PlayerAttackImpactRequestOutcome.ImpactNotDue;
            }

            WeaponDefinition weapon = _weaponCatalog.Get(attack.WeaponKind);
            InteractionCorrelation correlation = _interactionLedger.Open(InteractionKind.PlayerAttack, Revision);

            _pendingAttackImpactRequest = new PlayerAttackImpactRequest(
                correlation,
                attack.Id,
                attack.WeaponKind,
                _player.Position,
                aimDirection,
                weapon.Range,
                weapon.Arc);

            _playerAttackImpactRequestOutcome.Fill(
                PlayerAttackImpactRequestStatus.Requested,
                _pendingAttackImpactRequest);

            return _playerAttackImpactRequestOutcome;
        }

        public PlayerAttackImpactOutcome ApplyPlayerAttackImpact(PlayerAttackImpactResolution resolution)
        {
            if (resolution == null)
            {
                throw new ArgumentNullException(nameof(resolution));
            }

            InteractionAdmission admission = _interactionLedger.Admit(resolution, Revision);

            if (admission.IsAdmitted == false)
            {
                return FillImpactRejection(MapAttackRejectionReason(admission.RejectionReason));
            }

            PlayerAttackImpactRequest request = _pendingAttackImpactRequest;

            if (request.WeaponKind == WeaponKind.Ranged && resolution.HitEnemies.Count > 1)
            {
                return FillImpactRejection(PlayerAttackImpactRejectionReason.TooManyTargets);
            }

            List<EnemyId> targets = _scratchTargets;
            targets.Clear();

            for (int index = 0; index < resolution.HitEnemies.Count; index++)
            {
                EnemyId enemyId = resolution.HitEnemies[index];

                if (_enemies.TryGetValue(enemyId, out Enemy enemy) == false)
                {
                    return FillImpactRejection(PlayerAttackImpactRejectionReason.UnknownTarget);
                }

                if (IsWithinAttackRange(enemy, request.Range) == false)
                {
                    return FillImpactRejection(PlayerAttackImpactRejectionReason.TargetOutOfRange);
                }

                if (IsWithinAttackArc(enemy, request) == false)
                {
                    return FillImpactRejection(PlayerAttackImpactRejectionReason.TargetOutsideArc);
                }

                if (targets.Contains(enemyId) == false)
                {
                    targets.Add(enemyId);
                }
            }

            _interactionLedger.Complete(resolution.Correlation.InteractionId);

            WeaponDefinition weapon = _weaponCatalog.Get(request.WeaponKind);
            PendingPlayerAttack attack = _player.PendingAttack;
            AggregateRevision nextRevision = Revision.Next();
            _operationEvents.Clear();
            List<EnemyId> defeated = _scratchDefeated;
            defeated.Clear();

            for (int index = 0; index < targets.Count; index++)
            {
                Enemy enemy = _enemies[targets[index]];
                DamageApplication application = enemy.TakeDamage(weapon.Damage);

                _operationEvents.Add(_eventPool.TakeEnemyDamaged(
                    Id,
                    nextRevision,
                    enemy.Id,
                    application.AppliedDamage,
                    application.RemainingHealth));

                if (enemy.IsDefeated)
                {
                    defeated.Add(enemy.Id);
                    _operationEvents.Add(_eventPool.TakeEnemyDefeated(
                        Id,
                        nextRevision,
                        enemy.Id,
                        enemy.Kind));

                    if (enemy.HasPendingAttack)
                    {
                        PendingEnemyAttack pendingEnemyAttack = enemy.PendingAttack;
                        enemy.CancelAttack();
                        _operationEvents.Add(_eventPool.TakeEnemyAttackCancelled(
                            Id,
                            nextRevision,
                            enemy.Id,
                            pendingEnemyAttack.Id,
                            AttackCancellationCause.AttackerDefeated));
                    }
                }
            }

            for (int index = 0; index < defeated.Count; index++)
            {
                Enemy enemy = _enemies[defeated[index]];

                if (enemy.Kind == EnemyKind.Boss)
                {
                    _currentWave.MarkBossDefeated();
                }

                _enemies.Remove(defeated[index]);
            }

            ProcessWaveProgressionAfterEnemyDeaths(nextRevision, _operationEvents);
            AttackOutcome outcome = targets.Count > 0 ? AttackOutcome.Hit : AttackOutcome.Miss;

            _operationEvents.Add(_eventPool.TakePlayerAttackCompleted(
                Id,
                nextRevision,
                _player.Id,
                attack.Id,
                attack.WeaponKind,
                outcome,
                CurrentTime));

            _player.CompleteAttack();
            Revision = nextRevision;

            _playerAttackImpactOutcome.Fill(
                outcome == AttackOutcome.Hit
                    ? PlayerAttackImpactStatus.Hit
                    : PlayerAttackImpactStatus.Missed,
                PlayerAttackImpactRejectionReason.None,
                CreateChange(true));

            return _playerAttackImpactOutcome;
        }

        private PlayerAttackImpactOutcome FillImpactRejection(
            PlayerAttackImpactRejectionReason reason)
        {
            _playerAttackImpactOutcome.Fill(
                PlayerAttackImpactStatus.Rejected,
                reason,
                CreateNoChange());

            return _playerAttackImpactOutcome;
        }

        public EnemyAttackStartOutcome StartEligibleEnemyAttacks()
        {
            if (Status != ArenaRunStatus.Playing)
            {
                return FillEnemyAttackStart(EnemyAttackStartStatus.RunIsNotPlaying);
            }

            if (_interactionLedger.HasPending)
            {
                return FillEnemyAttackStart(EnemyAttackStartStatus.InteractionPending);
            }

            List<EnemyId> orderedIds = OrderedEnemyIds();
            List<EnemyId> eligible = _scratchEligible;
            eligible.Clear();

            for (int index = 0; index < orderedIds.Count; index++)
            {
                Enemy enemy = _enemies[orderedIds[index]];

                if (enemy.CanStartAttack(CurrentTime) == false)
                {
                    continue;
                }

                if (IsPlayerWithinEnemyAttackRange(enemy) == false)
                {
                    continue;
                }

                eligible.Add(enemy.Id);
            }

            if (eligible.Count == 0)
            {
                return FillEnemyAttackStart(EnemyAttackStartStatus.NoEligibleEnemies);
            }

            AggregateRevision nextRevision = Revision.Next();
            AttackId nextAttackId = _lastAttackId;
            List<PendingEnemyAttack> startedAttacks = _scratchStartedAttacks;
            startedAttacks.Clear();
            _operationEvents.Clear();

            for (int index = 0; index < eligible.Count; index++)
            {
                Enemy enemy = _enemies[eligible[index]];
                nextAttackId = nextAttackId.Next();

                PendingEnemyAttack attack = enemy.StartAttack(nextAttackId, CurrentTime);
                startedAttacks.Add(attack);

                _operationEvents.Add(_eventPool.TakeEnemyAttackStarted(
                    Id,
                    nextRevision,
                    attack.EnemyId,
                    attack.Id,
                    attack.StartedAt,
                    attack.ImpactAt));
            }

            _lastAttackId = nextAttackId;
            Revision = nextRevision;

            _enemyAttackStartOutcome.Fill(
                EnemyAttackStartStatus.Started,
                startedAttacks,
                CreateChange(true));

            return _enemyAttackStartOutcome;
        }

        private EnemyAttackStartOutcome FillEnemyAttackStart(EnemyAttackStartStatus status)
        {
            _enemyAttackStartOutcome.Fill(
                status,
                Array.Empty<PendingEnemyAttack>(),
                CreateNoChange());

            return _enemyAttackStartOutcome;
        }

        public EnemyAttackImpactOutcome ResolveDueEnemyAttackImpacts()
        {
            if (Status != ArenaRunStatus.Playing)
            {
                return FillEnemyAttackImpact(EnemyAttackImpactStatus.RunIsNotPlaying, 0, 0);
            }

            if (_interactionLedger.HasPending)
            {
                return FillEnemyAttackImpact(EnemyAttackImpactStatus.InteractionPending, 0, 0);
            }

            List<EnemyId> orderedIds = OrderedEnemyIds();
            List<EnemyAttackImpactPlanEntry> plan = _scratchImpactPlan;
            plan.Clear();
            Health projectedHealth = _player.Health;
            bool playerWillBeDefeated = false;

            for (int index = 0; index < orderedIds.Count; index++)
            {
                Enemy enemy = _enemies[orderedIds[index]];

                if (enemy.HasPendingAttack == false)
                {
                    continue;
                }

                PendingEnemyAttack attack = enemy.PendingAttack;

                if (attack.IsReadyToImpact(CurrentTime) == false)
                {
                    continue;
                }

                if (playerWillBeDefeated)
                {
                    continue;
                }

                if (IsPlayerWithinEnemyAttackRange(enemy) == false)
                {
                    plan.Add(EnemyAttackImpactPlanEntry.Miss(enemy.Id, attack.Id, projectedHealth));
                    continue;
                }

                DamageApplication application = projectedHealth.ApplyDamage(enemy.Definition.AttackDamage);

                projectedHealth = application.RemainingHealth;
                plan.Add(EnemyAttackImpactPlanEntry.Hit(
                    enemy.Id,
                    attack.Id,
                    application.AppliedDamage,
                    application.RemainingHealth));

                if (application.IsLethal)
                {
                    playerWillBeDefeated = true;
                }
            }

            if (plan.Count == 0)
            {
                return FillEnemyAttackImpact(EnemyAttackImpactStatus.NoAttacksDue, 0, 0);
            }

            AggregateRevision nextRevision = Revision.Next();
            _operationEvents.Clear();
            int hitCount = 0;
            bool actualDefeatedPlayer = false;

            for (int index = 0; index < plan.Count; index++)
            {
                EnemyAttackImpactPlanEntry entry = plan[index];
                Enemy enemy = _enemies[entry.EnemyId];

                if (entry.IsHit)
                {
                    hitCount++;
                    DamageApplication application = _player.TakeDamage(enemy.Definition.AttackDamage);

                    _operationEvents.Add(_eventPool.TakePlayerDamaged(
                        Id,
                        nextRevision,
                        _player.Id,
                        entry.EnemyId,
                        entry.AttackId,
                        application.AppliedDamage,
                        application.RemainingHealth));

                    if (application.IsLethal)
                    {
                        _operationEvents.Add(_eventPool.TakePlayerDefeated(
                            Id,
                            nextRevision,
                            _player.Id));
                        actualDefeatedPlayer = true;
                    }
                }

                enemy.CompleteAttack();
                _operationEvents.Add(_eventPool.TakeEnemyAttackCompleted(
                    Id,
                    nextRevision,
                    entry.EnemyId,
                    entry.AttackId,
                    entry.Outcome,
                    CurrentTime));
            }

            if (playerWillBeDefeated && actualDefeatedPlayer)
            {
                Status = ArenaRunStatus.Defeat;
                CancelRemainingAttacksOnDefeat(orderedIds, nextRevision, _operationEvents);
                _operationEvents.Add(_eventPool.TakeArenaRunDefeated(Id, nextRevision));
            }

            Revision = nextRevision;

            return FillEnemyAttackImpact(
                EnemyAttackImpactStatus.Resolved,
                plan.Count,
                hitCount);
        }

        private EnemyAttackImpactOutcome FillEnemyAttackImpact(
            EnemyAttackImpactStatus status,
            int resolvedAttackCount,
            int hitCount)
        {
            _enemyAttackImpactOutcome.Fill(
                status,
                resolvedAttackCount,
                hitCount,
                status == EnemyAttackImpactStatus.Resolved
                    ? CreateChange(true)
                    : CreateNoChange());

            return _enemyAttackImpactOutcome;
        }

        public EnemyMovementBatchRequestOutcome RequestEnemyMovementBatch(GameDuration duration)
        {
            if (Status != ArenaRunStatus.Playing)
            {
                return EnemyMovementBatchRequestOutcome.RunIsNotPlaying;
            }

            if (_interactionLedger.HasPending)
            {
                throw new InvalidOperationException("An interaction is already pending.");
            }

            if (duration.Seconds <= 0d)
            {
                return EnemyMovementBatchRequestOutcome.NoEligibleEnemies;
            }

            List<EnemyId> orderedIds = OrderedEnemyIds();
            List<EnemyMovementIntent> intents = _scratchMovementIntents;
            intents.Clear();

            for (int index = 0; index < orderedIds.Count; index++)
            {
                Enemy enemy = _enemies[orderedIds[index]];

                if (enemy.HasPendingAttack)
                {
                    continue;
                }

                if (IsPlayerWithinEnemyAttackRange(enemy))
                {
                    continue;
                }

                Displacement3D toPlayer = new(
                    _player.Position.X - enemy.Position.X,
                    0f,
                    _player.Position.Z - enemy.Position.Z);

                bool hasDirection = toPlayer.TryToDirection(out Direction3D direction);

                if (hasDirection == false)
                {
                    continue;
                }

                Distance requestedDistance = enemy.Definition.MovementSpeed.DistanceOver(duration);

                if (requestedDistance.IsZero)
                {
                    continue;
                }

                PlanarMovementIntent movementIntent = new(
                    enemy.Position,
                    direction,
                    requestedDistance);

                EnemyMovementIntent enemyIntent = new(
                    enemy.Id,
                    movementIntent,
                    enemy.CollisionRadius);

                intents.Add(enemyIntent);
            }

            if (intents.Count == 0)
            {
                return EnemyMovementBatchRequestOutcome.NoEligibleEnemies;
            }

            InteractionCorrelation correlation = _interactionLedger.Open(InteractionKind.EnemyMovementBatch, Revision);
            _movementBatchRequest.Fill(correlation, intents);
            _pendingEnemyMovementBatchRequest = _movementBatchRequest;
            _enemyMovementBatchRequestOutcome.Fill(
                EnemyMovementBatchRequestStatus.Requested,
                _pendingEnemyMovementBatchRequest);

            return _enemyMovementBatchRequestOutcome;
        }

        public EnemyMovementBatchResolutionOutcome ApplyEnemyMovementBatch(EnemyMovementBatchResolution resolution)
        {
            if (resolution == null)
            {
                throw new ArgumentNullException(nameof(resolution));
            }

            InteractionAdmission admission = _interactionLedger.Admit(resolution, Revision);

            if (admission.IsAdmitted == false)
            {
                return FillMovementBatch(
                    EnemyMovementBatchResolutionStatus.Rejected,
                    MapEnemyMovementRejectionReason(admission.RejectionReason));
            }

            EnemyMovementBatchRejectionReason validation = ValidateEnemyMovementBatchResolution(resolution, out Dictionary<EnemyId, Position3D> acceptedPositions);

            if (validation != EnemyMovementBatchRejectionReason.None)
            {
                return FillMovementBatch(
                    EnemyMovementBatchResolutionStatus.Rejected,
                    validation);
            }

            _interactionLedger.Complete(resolution.Correlation.InteractionId);
            IReadOnlyList<EnemyMovementIntent> pendingIntents = _pendingEnemyMovementBatchRequest.Intents;
            bool anyMoved = false;

            for (int index = 0; index < pendingIntents.Count; index++)
            {
                EnemyMovementIntent intent = pendingIntents[index];
                Enemy enemy = _enemies[intent.EnemyId];
                Position3D acceptedPosition = acceptedPositions[intent.EnemyId];

                if (enemy.Position != acceptedPosition)
                {
                    anyMoved = true;
                }
            }

            if (anyMoved == false)
            {
                return FillMovementBatch(
                    EnemyMovementBatchResolutionStatus.AcceptedWithoutStateChange,
                    EnemyMovementBatchRejectionReason.None);
            }

            for (int index = 0; index < pendingIntents.Count; index++)
            {
                EnemyMovementIntent intent = pendingIntents[index];
                Enemy enemy = _enemies[intent.EnemyId];
                enemy.MoveTo(acceptedPositions[intent.EnemyId]);
            }

            Revision = Revision.Next();

            return FillMovementBatch(
                EnemyMovementBatchResolutionStatus.Applied,
                EnemyMovementBatchRejectionReason.None);
        }

        private EnemyMovementBatchResolutionOutcome FillMovementBatch(
            EnemyMovementBatchResolutionStatus status,
            EnemyMovementBatchRejectionReason rejectionReason)
        {
            _enemyMovementBatchResolutionOutcome.Fill(
                status,
                rejectionReason,
                status == EnemyMovementBatchResolutionStatus.Applied
                    ? CreateStateChange()
                    : CreateNoChange());

            return _enemyMovementBatchResolutionOutcome;
        }

        public InteractionCancellationOutcome CancelPendingInteraction(
            InteractionCorrelation correlation,
            InteractionCancellationReason reason)
        {
            if (_interactionLedger.HasPending == false)
            {
                _cancellationOutcome.Fill(
                    InteractionCancellationStatus.NoPendingInteraction,
                    reason);

                return _cancellationOutcome;
            }

            bool cancelled = _interactionLedger.Cancel(correlation);

            if (cancelled == false)
            {
                _cancellationOutcome.Fill(
                    InteractionCancellationStatus.CorrelationMismatch,
                    reason);

                return _cancellationOutcome;
            }

            _cancellationOutcome.Fill(
                InteractionCancellationStatus.Cancelled,
                reason);

            return _cancellationOutcome;
        }

        public GameTimePoint WeaponReadyAt(WeaponKind kind)
        {
            return _player.ReadyAt(kind);
        }

        public ArenaRunStateSnapshot CreateSnapshot()
        {
            List<EnemyStateSnapshot> enemies = new(_enemies.Count);
            List<EnemyId> orderedIds = OrderedEnemyIds();

            for (int index = 0; index < orderedIds.Count; index++)
            {
                Enemy enemy = _enemies[orderedIds[index]];

                enemies.Add(
                    new EnemyStateSnapshot(
                        enemy.Id,
                        enemy.Kind,
                        enemy.Position,
                        enemy.Health,
                        enemy.CollisionRadius,
                        enemy.AttackReadyAt,
                        enemy.PendingAttack));
            }

            PlayerStateSnapshot player = new(
                _player.Id,
                _player.Position,
                _player.Health,
                _player.SelectedWeapon,
                _player.ReadyAt(WeaponKind.Ranged),
                _player.ReadyAt(WeaponKind.Melee),
                _player.PendingAttack);

            WaveStateSnapshot wave = new(
                _currentWave.Number,
                _currentWave.Phase,
                _currentWave.RegularEnemiesRemainingToSpawn,
                _currentWave.BossStatus);

            return new ArenaRunStateSnapshot(
                Id,
                Revision,
                Status,
                CurrentTime,
                _arenaDefinition.Bounds,
                player,
                enemies,
                wave,
                _waveCatalog.Last);
        }

        /// <summary>
        /// Allocates an empty snapshot shell sized for this run's enemy
        /// capacity. The tick path fills it in place on every tick.
        /// </summary>
        public ArenaRunStateSnapshot CreateSnapshotBuffer()
        {
            return new ArenaRunStateSnapshot(EnemyCapacity);
        }

        /// <summary>
        /// Fills <paramref name="target"/> in place and returns it. Falls back
        /// to an allocating snapshot when no target is supplied.
        /// </summary>
        public ArenaRunStateSnapshot CreateSnapshot(ArenaRunStateSnapshot target)
        {
            if (target == null || target.EnemyBuffer == null)
            {
                return CreateSnapshot();
            }

            FillStateSnapshot(target);

            return target;
        }

        private void FillStateSnapshot(ArenaRunStateSnapshot target)
        {
            List<EnemyStateSnapshot> enemies = target.EnemyBuffer;
            enemies.Clear();
            List<EnemyId> orderedIds = OrderedEnemyIds();

            for (int index = 0; index < orderedIds.Count; index++)
            {
                Enemy enemy = _enemies[orderedIds[index]];
                EnemyStateSnapshot slot = target.GetOrCreateEnemySlot(index);

                slot.Fill(
                    enemy.Id,
                    enemy.Kind,
                    enemy.Position,
                    enemy.Health,
                    enemy.CollisionRadius,
                    enemy.AttackReadyAt,
                    enemy.PendingAttack);

                enemies.Add(slot);
            }

            target.PlayerSlot.Fill(
                _player.Id,
                _player.Position,
                _player.Health,
                _player.SelectedWeapon,
                _player.ReadyAt(WeaponKind.Ranged),
                _player.ReadyAt(WeaponKind.Melee),
                _player.PendingAttack);

            target.WaveSlot.Fill(
                _currentWave.Number,
                _currentWave.Phase,
                _currentWave.RegularEnemiesRemainingToSpawn,
                _currentWave.BossStatus);

            target.FillMeta(
                Id,
                Revision,
                Status,
                CurrentTime,
                _arenaDefinition.Bounds,
                _waveCatalog.Last);
        }

        private static PlayerMovementResolutionRejectionReason MapRejectionReason(InteractionRejectionReason reason)
        {
            return reason switch
            {
                InteractionRejectionReason.ForeignArenaRun => PlayerMovementResolutionRejectionReason.ForeignArenaRun,
                InteractionRejectionReason.UnknownInteraction => PlayerMovementResolutionRejectionReason.UnknownInteraction,
                InteractionRejectionReason.InteractionClosed => PlayerMovementResolutionRejectionReason.InteractionClosed,
                InteractionRejectionReason.StaleRevision => PlayerMovementResolutionRejectionReason.StaleRevision,
                InteractionRejectionReason.KindMismatch => PlayerMovementResolutionRejectionReason.KindMismatch,
                InteractionRejectionReason.None => throw new ArgumentOutOfRangeException(nameof(reason)),
                _ => throw new ArgumentOutOfRangeException(nameof(reason)),
            };
        }

        private static EnemySpawnResolutionRejectionReason MapSpawnRejectionReason(InteractionRejectionReason reason)
        {
            return reason switch
            {
                InteractionRejectionReason.ForeignArenaRun => EnemySpawnResolutionRejectionReason.ForeignArenaRun,
                InteractionRejectionReason.UnknownInteraction => EnemySpawnResolutionRejectionReason.UnknownInteraction,
                InteractionRejectionReason.InteractionClosed => EnemySpawnResolutionRejectionReason.InteractionClosed,
                InteractionRejectionReason.StaleRevision => EnemySpawnResolutionRejectionReason.StaleRevision,
                InteractionRejectionReason.KindMismatch => EnemySpawnResolutionRejectionReason.KindMismatch,
                _ => throw new ArgumentOutOfRangeException(nameof(reason)),
            };
        }

        private static PlayerAttackImpactRejectionReason MapAttackRejectionReason(
            InteractionRejectionReason reason)
        {
            return reason switch
            {
                InteractionRejectionReason.ForeignArenaRun => PlayerAttackImpactRejectionReason.ForeignArenaRun,
                InteractionRejectionReason.UnknownInteraction => PlayerAttackImpactRejectionReason.UnknownInteraction,
                InteractionRejectionReason.InteractionClosed => PlayerAttackImpactRejectionReason.InteractionClosed,
                InteractionRejectionReason.StaleRevision => PlayerAttackImpactRejectionReason.StaleRevision,
                InteractionRejectionReason.KindMismatch => PlayerAttackImpactRejectionReason.KindMismatch,
                _ => throw new ArgumentOutOfRangeException(nameof(reason)),
            };
        }

        private static EnemyMovementBatchRejectionReason MapEnemyMovementPathVerdict(MovementPathVerdict verdict)
        {
            return verdict switch
            {
                MovementPathVerdict.Accepted => EnemyMovementBatchRejectionReason.None,
                MovementPathVerdict.OffGroundPlane => EnemyMovementBatchRejectionReason.AcceptedPositionOffGroundPlane,
                MovementPathVerdict.BehindOrigin => EnemyMovementBatchRejectionReason.AcceptedPositionBehindRequest,
                MovementPathVerdict.BeyondRequestedDistance => EnemyMovementBatchRejectionReason.AcceptedPositionBeyondRequestedDistance,
                MovementPathVerdict.OffMovementPath => EnemyMovementBatchRejectionReason.AcceptedPositionOffMovementPath,
                _ => throw new ArgumentOutOfRangeException(nameof(verdict)),
            };
        }

        private static EnemyMovementBatchRejectionReason MapEnemyMovementRejectionReason(InteractionRejectionReason reason)
        {
            return reason switch
            {
                InteractionRejectionReason.ForeignArenaRun => EnemyMovementBatchRejectionReason.ForeignArenaRun,
                InteractionRejectionReason.UnknownInteraction => EnemyMovementBatchRejectionReason.UnknownInteraction,
                InteractionRejectionReason.InteractionClosed => EnemyMovementBatchRejectionReason.InteractionClosed,
                InteractionRejectionReason.StaleRevision => EnemyMovementBatchRejectionReason.StaleRevision,
                InteractionRejectionReason.KindMismatch => EnemyMovementBatchRejectionReason.KindMismatch,
                _ => throw new ArgumentOutOfRangeException(nameof(reason)),
            };
        }

        private static int CompareEnemyIds(EnemyId left, EnemyId right)
        {
            return left.Value.CompareTo(right.Value);
        }

        private static int CalculateEnemyCapacity(WaveCatalog waveCatalog)
        {
            int capacity = 1;
            int waveCount = waveCatalog.Last.Value;

            for (int number = 1; number <= waveCount; number++)
            {
                int waveSize =
                    waveCatalog.Get(WaveNumber.FromValue(number)).RegularEnemyCount + 1;

                if (waveSize > capacity)
                {
                    capacity = waveSize;
                }
            }

            return capacity;
        }

        private PlayerMovementResolutionRejectionReason ValidateAcceptedPosition(PlayerMovementRequest pendingMovementRequest, Position3D acceptedPosition)
        {
            MovementPathVerdict verdict = _movementPathPolicy.Validate(pendingMovementRequest.Intent, acceptedPosition);

            switch (verdict)
            {
                case MovementPathVerdict.Accepted:
                    break;

                case MovementPathVerdict.OffGroundPlane:
                    return PlayerMovementResolutionRejectionReason.AcceptedPositionOffGroundPlane;

                case MovementPathVerdict.BehindOrigin:
                    return PlayerMovementResolutionRejectionReason.AcceptedPositionBehindRequest;

                case MovementPathVerdict.BeyondRequestedDistance:
                    return PlayerMovementResolutionRejectionReason.AcceptedPositionBeyondRequestedDistance;

                case MovementPathVerdict.OffMovementPath:
                    return PlayerMovementResolutionRejectionReason.AcceptedPositionOffMovementPath;

                default:
                    throw new ArgumentOutOfRangeException(nameof(verdict));
            }

            if (_arenaDefinition.Bounds.Contains(acceptedPosition, _player.CollisionRadius) == false)
            {
                return PlayerMovementResolutionRejectionReason.AcceptedPositionOutsideArena;
            }

            return PlayerMovementResolutionRejectionReason.None;
        }

        private EnemyMovementBatchRejectionReason ValidateEnemyMovementBatchResolution(EnemyMovementBatchResolution resolution, out Dictionary<EnemyId, Position3D> acceptedPositions)
        {
            acceptedPositions = _scratchAcceptedPositions;
            acceptedPositions.Clear();
            _scratchExpectedIds.Clear();

            IReadOnlyList<EnemyMovementIntent> pendingIntents = _pendingEnemyMovementBatchRequest.Intents;

            for (int index = 0; index < pendingIntents.Count; index++)
            {
                _scratchExpectedIds.Add(pendingIntents[index].EnemyId);
            }

            for (int index = 0; index < resolution.Entries.Count; index++)
            {
                EnemyMovementBatchResolutionEntry entry = resolution.Entries[index];

                if (entry.EnemyId.IsNone)
                {
                    return EnemyMovementBatchRejectionReason.EnemyIdIsNone;
                }

                if (_scratchExpectedIds.Contains(entry.EnemyId) == false)
                {
                    return EnemyMovementBatchRejectionReason.UnexpectedEnemyId;
                }

                if (acceptedPositions.ContainsKey(entry.EnemyId))
                {
                    return EnemyMovementBatchRejectionReason.DuplicateEnemyId;
                }

                acceptedPositions.Add(entry.EnemyId, entry.AcceptedPosition);
            }

            for (int index = 0; index < pendingIntents.Count; index++)
            {
                EnemyMovementIntent intent = pendingIntents[index];

                if (acceptedPositions.ContainsKey(intent.EnemyId) == false)
                {
                    return EnemyMovementBatchRejectionReason.MissingEnemyId;
                }

                if (_enemies.ContainsKey(intent.EnemyId) == false)
                {
                    return EnemyMovementBatchRejectionReason.EnemyNoLongerActive;
                }

                MovementPathVerdict verdict = _movementPathPolicy.Validate(intent.Intent, acceptedPositions[intent.EnemyId]);
                EnemyMovementBatchRejectionReason pathReason = MapEnemyMovementPathVerdict(verdict);

                if (pathReason != EnemyMovementBatchRejectionReason.None)
                {
                    return pathReason;
                }
            }

            return EnemyMovementBatchRejectionReason.None;
        }

        private void CancelRemainingAttacksOnDefeat(
            List<EnemyId> orderedIds,
            AggregateRevision nextRevision,
            List<IArenaDomainEvent> events)
        {
            for (int index = 0; index < orderedIds.Count; index++)
            {
                Enemy enemy = _enemies[orderedIds[index]];

                if (enemy.HasPendingAttack == false)
                {
                    continue;
                }

                PendingEnemyAttack attack = enemy.PendingAttack;
                enemy.CancelAttack();
                events.Add(_eventPool.TakeEnemyAttackCancelled(
                    Id,
                    nextRevision,
                    enemy.Id,
                    attack.Id,
                    AttackCancellationCause.ArenaRunTerminated));
            }

            if (_player.HasPendingAttack)
            {
                PendingPlayerAttack attack = _player.PendingAttack;
                _player.CancelAttack();
                events.Add(_eventPool.TakePlayerAttackCancelled(
                    Id,
                    nextRevision,
                    _player.Id,
                    attack.Id,
                    AttackCancellationCause.AttackerDefeated));
            }
        }

        private int CountActiveRegularEnemies()
        {
            int count = 0;

            foreach (Enemy enemy in _enemies.Values)
            {
                if (enemy.Kind == EnemyKind.Regular)
                {
                    count++;
                }
            }

            return count;
        }

        private ArenaRunChange CreateNoChange()
        {
            _change.Fill(Revision, false, s_noEvents);

            return _change;
        }

        private ArenaRunChange CreateStateChange()
        {
            _change.Fill(Revision, true, s_noEvents);

            return _change;
        }

        private ArenaRunChange CreateChange(bool hasStateChange)
        {
            _change.Fill(Revision, hasStateChange, _operationEvents);

            return _change;
        }

        private bool IsWithinAttackRange(Enemy enemy, Distance range)
        {
            float distance = _player.Position.GroundDistanceTo(enemy.Position);
            float limit = range.Value
                + enemy.CollisionRadius.Value.Value
                + GeometryTolerance.CombatRangeTolerance;

            return distance <= limit;
        }

        private bool IsWithinAttackArc(Enemy enemy, PlayerAttackImpactRequest request)
        {
            if (request.Arc.IsFull)
            {
                return true;
            }

            Displacement3D offset = enemy.Position - request.Origin;
            Displacement3D horizontalOffset = new(offset.X, 0f, offset.Z);

            if (horizontalOffset.TryToDirection(out Direction3D offsetDirection) == false)
            {
                return true;
            }

            float alignment = offsetDirection.Dot(request.AimDirection);

            return alignment >= request.Arc.CosHalfAngle - GeometryTolerance.AttackArcTolerance;
        }

        private List<EnemyId> OrderedEnemyIds()
        {
            _orderedEnemyIds.Clear();

            foreach (EnemyId enemyId in _enemies.Keys)
            {
                _orderedEnemyIds.Add(enemyId);
            }

            _orderedEnemyIds.Sort(s_compareEnemyIdValues);

            return _orderedEnemyIds;
        }

        private bool IsPlayerWithinEnemyAttackRange(Enemy enemy)
        {
            float distance = enemy.Position.GroundDistanceTo(_player.Position);
            float limit = enemy.Definition.AttackRange.Value
                + enemy.CollisionRadius.Value.Value
                + _player.CollisionRadius.Value.Value
                + GeometryTolerance.CombatRangeTolerance;

            return distance <= limit;
        }

        private void ProcessWaveProgressionAfterEnemyDeaths(AggregateRevision nextRevision, List<IArenaDomainEvent> events)
        {
            WavePhase previousPhase = _currentWave.Phase;

            bool enteredBossCombat = _currentWave.TryEnterBossCombat(CountActiveRegularEnemies());

            if (enteredBossCombat)
            {
                events.Add(_eventPool.TakeWavePhaseChanged(
                    Id,
                    nextRevision,
                    _currentWave.Number,
                    previousPhase,
                    _currentWave.Phase));
            }

            previousPhase = _currentWave.Phase;
            bool completed = _currentWave.TryCompleteAfterBossDefeated();

            if (completed == false)
            {
                return;
            }

            events.Add(_eventPool.TakeWavePhaseChanged(
                Id,
                nextRevision,
                _currentWave.Number,
                previousPhase,
                _currentWave.Phase));

            events.Add(_eventPool.TakeWaveCompleted(Id, nextRevision, _currentWave.Number));

            if (_waveCatalog.IsLast(_currentWave.Number))
            {
                Status = ArenaRunStatus.Victory;
                events.Add(_eventPool.TakeArenaRunVictorious(Id, nextRevision));

                return;
            }

            StartNextWave(nextRevision, events);
        }

        private void StartNextWave(
            AggregateRevision nextRevision,
            List<IArenaDomainEvent> events)
        {
            WaveNumber nextNumber = _currentWave.Number.Next();
            _currentWave = new Wave(_waveCatalog.Get(nextNumber));

            events.Add(_eventPool.TakeWaveStarted(
                Id,
                nextRevision,
                _currentWave.Number,
                _currentWave.Phase));
        }
    }
}

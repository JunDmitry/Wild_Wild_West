using System;
using System.Collections.Generic;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Interactions.Spawn;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRun
    {
        private readonly ArenaDefinition _arenaDefinition;
        private readonly EnemyCatalog _enemyCatalog;
        private readonly WaveCatalog _waveCatalog;
        private readonly WeaponCatalog _weaponCatalog;

        private readonly Player _player;
        private readonly Dictionary<EnemyId, Enemy> _enemies;
        private readonly PendingInteractionLedger _interactionLedger;

        private Wave _currentWave;
        private PlayerMovementRequest _pendingMovementRequest;
        private EnemySpawnRequest _pendingSpawnRequest;
        private AttackId _lastAttackId;

        internal ArenaRun(
            ArenaRunId id,
            Player runPlayer,
            Wave runWave,
            ArenaDefinition runArenaDefinition,
            WeaponCatalog runWeaponCatalog,
            EnemyCatalog runEnemyCatalog,
            WaveCatalog runWaveCatalog)
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

            CurrentTime = new GameTimePoint(0);
            _currentWave.TryEnterBossCombat(0);
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

        public bool ContainsEnemy(EnemyId enemyId)
        {
            return _enemies.ContainsKey(enemyId);
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

            Distance pemittedDistance = _arenaDefinition.Bounds.PermittedTravel(_player.Position, direction, _player.CollisionRadius, requestedDistance);

            if (pemittedDistance.IsZero)
            {
                return PlayerMovementRequestOutcome.PositionUnchanged;
            }

            InteractionCorrelation correlation = _interactionLedger.Open(InteractionKind.PlayerMovement, Revision);
            _pendingMovementRequest = new(correlation, _player.Position, direction, pemittedDistance, _player.CollisionRadius);

            return PlayerMovementRequestOutcome.Requested(_pendingMovementRequest);
        }

        public WeaponSwitchOutcome SwitchWeapon()
        {
            if (Status != ArenaRunStatus.Playing)
            {
                return WeaponSwitchOutcome.RunIsNotPlaying(SelectedWeapon, CreateNoChange());
            }

            if (_interactionLedger.HasPending)
            {
                return WeaponSwitchOutcome.InteractionPending(SelectedWeapon, CreateNoChange());
            }

            if (_player.HasPendingAttack)
            {
                return WeaponSwitchOutcome.AttackPending(SelectedWeapon, CreateNoChange());
            }

            _player.SwitchWeapon();
            Revision = Revision.Next();

            return WeaponSwitchOutcome.Switched(SelectedWeapon, CreateStateChange());
        }

        public TimeAdvanceOutcome AdvanceTime(GameDuration duration)
        {
            if (duration.Seconds <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            if (Status != ArenaRunStatus.Playing)
            {
                return TimeAdvanceOutcome.RunIsNotPlaying(CurrentTime, CreateNoChange());
            }

            if (_interactionLedger.HasPending)
            {
                return TimeAdvanceOutcome.InteractionPending(CurrentTime, CreateNoChange());
            }

            CurrentTime += duration;
            Revision = Revision.Next();

            return TimeAdvanceOutcome.Advanced(CurrentTime, CreateStateChange());
        }

        public PlayerMovementResolutionOutcome ApplyPlayerMovement(PlayerMovementResolution resolution)
        {
            InteractionAdmission admission = _interactionLedger.Admit(resolution, Revision);

            if (admission.IsAdmitted == false)
            {
                return PlayerMovementResolutionOutcome.Rejected(MapRejectionReason(admission.RejectionReason), CreateNoChange());
            }

            PlayerMovementResolutionRejectionReason geometryRejection = ValidateAcceptedPosition(_pendingMovementRequest, resolution.AcceptedPosition);

            if (geometryRejection != PlayerMovementResolutionRejectionReason.None)
            {
                return PlayerMovementResolutionOutcome.Rejected(geometryRejection, CreateNoChange());
            }

            _interactionLedger.Complete(resolution.Correlation.InteractionId);

            if (_player.Position == resolution.AcceptedPosition)
            {
                return PlayerMovementResolutionOutcome.AcceptedWithoutStateChange(CreateNoChange());
            }

            _player.MoveTo(resolution.AcceptedPosition);
            Revision = Revision.Next();

            return PlayerMovementResolutionOutcome.Applied(CreateStateChange());
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

            return EnemySpawnRequestOutcome.Requested(_pendingSpawnRequest);
        }

        public EnemySpawnResolutionOutcome ApplyEnemySpawn(EnemySpawnResolution resolution)
        {
            InteractionAdmission admission = _interactionLedger.Admit(resolution, Revision);

            if (admission.IsAdmitted == false)
            {
                return EnemySpawnResolutionOutcome.Rejected(MapSpawnRejectionReason(admission.RejectionReason), CreateNoChange());
            }

            if (resolution.EnemyId.IsNone)
            {
                return EnemySpawnResolutionOutcome.Rejected(EnemySpawnResolutionRejectionReason.EnemyIdIsNone, CreateNoChange());
            }

            if (_enemies.ContainsKey(resolution.EnemyId))
            {
                return EnemySpawnResolutionOutcome.Rejected(EnemySpawnResolutionRejectionReason.DuplicateEnemyId, CreateNoChange());
            }

            if (_arenaDefinition.IsSpawnPosition(resolution.SpawnPosition) == false)
            {
                return EnemySpawnResolutionOutcome.Rejected(EnemySpawnResolutionRejectionReason.SpawnPositionOutsidePerimeter, CreateNoChange());
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

            IArenaDomainEvent[] events =
            {
                new EnemySpawned(Id, Revision, enemy.Id, enemy.Kind, enemy.Position),
            };

            return EnemySpawnResolutionOutcome.Spawned(new ArenaRunChange(Revision, true, events));
        }

        public PlayerAttackStartOutcome StartPlayerAttack()
        {
            if (Status != ArenaRunStatus.Playing)
            {
                return PlayerAttackStartOutcome.RunIsNotPlaying(CreateNoChange());
            }

            if (_interactionLedger.HasPending)
            {
                return PlayerAttackStartOutcome.InteractionPending(CreateNoChange());
            }

            if (_player.HasPendingAttack)
            {
                return PlayerAttackStartOutcome.AttackAlreadyPending(CreateNoChange());
            }

            if (_player.IsWeaponReady(SelectedWeapon, CurrentTime) == false)
            {
                return PlayerAttackStartOutcome.WeaponNotReady(CreateNoChange());
            }

            WeaponDefinition weapon = _weaponCatalog.Get(SelectedWeapon);
            AttackId nextAttackId = _lastAttackId.Next();
            PendingAttack pendingAttack = _player.StartAttack(nextAttackId, weapon, CurrentTime);

            _lastAttackId = nextAttackId;
            Revision = Revision.Next();

            IArenaDomainEvent[] events =
            {
                new PlayerAttackStarted(
                    Id,
                    Revision,
                    _player.Id,
                    pendingAttack.Id,
                    pendingAttack.WeaponKind,
                    pendingAttack.StartedAt,
                    pendingAttack.ImpactAt)
            };

            return PlayerAttackStartOutcome.Started(pendingAttack, new ArenaRunChange(Revision, true, events));
        }

        public InteractionCancellationOutcome CancelPendingInteraction(
            InteractionCorrelation correlation,
            InteractionCancellationReason reason)
        {
            if (_interactionLedger.HasPending == false)
            {
                return InteractionCancellationOutcome.NoPendingInteraction(reason);
            }

            bool cancelled = _interactionLedger.Cancel(correlation);

            if (cancelled == false)
            {
                return InteractionCancellationOutcome.CorrelationMismatch(reason);
            }

            return InteractionCancellationOutcome.Cancelled(reason);
        }

        public GameTimePoint WeaponReadyAt(WeaponKind kind)
        {
            return _player.ReadyAt(kind);
        }

        private PlayerMovementResolutionRejectionReason ValidateAcceptedPosition(PlayerMovementRequest pendingMovementRequest, Position3D acceptedPosition)
        {
            float tolerance = GeometryTolerance.MovementPathTolerance;
            Displacement3D travel = acceptedPosition - pendingMovementRequest.From;
            float along = travel.Dot(pendingMovementRequest.Direction);

            if (along < -tolerance)
            {
                return PlayerMovementResolutionRejectionReason.AcceptedPositionBehindRequest;
            }

            if (along > pendingMovementRequest.RequestedDistance.Value + tolerance)
            {
                return PlayerMovementResolutionRejectionReason.AcceptedPositionBeyondRequestedDistance;
            }

            float lateralSquared = travel.LengthSquared - (along * along);

            if (lateralSquared > tolerance * tolerance)
            {
                return PlayerMovementResolutionRejectionReason.AcceptedPositionOffMovementPath;
            }

            if (_arenaDefinition.Bounds.Contains(acceptedPosition, _player.CollisionRadius) == false)
            {
                return PlayerMovementResolutionRejectionReason.AcceptedPositionOutsideArena;
            }

            return PlayerMovementResolutionRejectionReason.None;
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
            return new ArenaRunChange(Revision, false, Array.Empty<IArenaDomainEvent>());
        }

        private ArenaRunChange CreateStateChange()
        {
            return new ArenaRunChange(Revision, true, Array.Empty<IArenaDomainEvent>());
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
    }
}

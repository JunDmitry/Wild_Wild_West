using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRun
    {
        private readonly Player _player;
        private readonly Wave _currentWave;
        private readonly ArenaBounds _arenaBounds;
        private readonly PendingInteractionLedger _interactionLedger;

        private PlayerMovementRequest _pendingMovementRequest;

        internal ArenaRun(
            ArenaRunId id,
            Player player,
            Wave currentWave,
            ArenaBounds arenaBounds)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(id));
            }

            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (arenaBounds.CanContain(player.CollisionRadius) == false)
            {
                throw new ArgumentException("Arena cannot contain the Player.", nameof(arenaBounds));
            }

            if (arenaBounds.Contains(player.Position, player.CollisionRadius) == false)
            {
                throw new ArgumentException("Player position is outside the Arena.", nameof(player));
            }

            Id = id;
            _player = player;
            _currentWave = currentWave ?? throw new ArgumentNullException(nameof(currentWave));
            _arenaBounds = arenaBounds;
            Status = ArenaRunStatus.Playing;
            Revision = AggregateRevision.Initial;
            _interactionLedger = new PendingInteractionLedger(id);
        }

        public ArenaRunId Id { get; }
        public ArenaRunStatus Status { get; private set; }
        public AggregateRevision Revision { get; private set; }

        public bool HasPendingInteraction => _interactionLedger.HasPending;
        public PlayerId PlayerId => _player.Id;
        public Position3D PlayerPosition => _player.Position;
        public WaveNumber CurrentWaveNumber => _currentWave.Number;
        public WavePhase CurrentWavePhase => _currentWave.Phase;
        public ArenaBounds ArenaBounds => _arenaBounds;

        public PlayerMovementRequestOutcome RequestPlayerMovement(MovementInput movementInput, GameDuration duration)
        {
            if (Status != ArenaRunStatus.Playing)
            {
                return PlayerMovementRequestOutcome.NotPlaying;
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

            Distance pemittedDistance = _arenaBounds.PermittedTravel(_player.Position, direction, _player.CollisionRadius, requestedDistance);

            if (pemittedDistance.IsZero)
            {
                return PlayerMovementRequestOutcome.PositionUnchanged;
            }

            InteractionCorrelation correlation = _interactionLedger.Open(InteractionKind.PlayerMovement, Revision);
            _pendingMovementRequest = new(correlation, _player.Position, direction, pemittedDistance, _player.CollisionRadius);

            return PlayerMovementRequestOutcome.Requested(_pendingMovementRequest);
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

            if (_arenaBounds.Contains(acceptedPosition, _player.CollisionRadius) == false)
            {
                return PlayerMovementResolutionRejectionReason.AcceptedPositionOutsideArena;
            }

            return PlayerMovementResolutionRejectionReason.None;
        }

        private ArenaRunChange CreateNoChange()
        {
            return new ArenaRunChange(Revision, false, Array.Empty<IDomainEvent>());
        }

        private ArenaRunChange CreateStateChange()
        {
            return new ArenaRunChange(Revision, true, Array.Empty<IDomainEvent>());
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
    }
}

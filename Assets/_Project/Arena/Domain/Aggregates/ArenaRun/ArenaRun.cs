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

            double distance = _player.MovementSpeed.UnitsPerSecond * duration.Seconds * movementInput.Magnitude;

            if (double.IsNaN(distance))
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            if (double.IsInfinity(distance))
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            if (distance > float.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            Position3D requestedPosition = _player.Position.MovedAlong(direction, (float)distance);
            requestedPosition = _arenaBounds.Clamp(requestedPosition, _player.CollisionRadius);

            if (requestedPosition == _player.Position)
            {
                return PlayerMovementRequestOutcome.PositionUnchanged;
            }

            InteractionCorrelation correlation = _interactionLedger.Open(InteractionKind.PlayerMovement, Revision);
            PlayerMovementRequest request = new(correlation, _player.Position, requestedPosition, _player.CollisionRadius);

            return PlayerMovementRequestOutcome.Requested(request);
        }

        public PlayerMovementResolutionOutcome ApplyPlayerMovement(PlayerMovementResolution resolution)
        {
            InteractionAdmission admission = _interactionLedger.Admit(resolution, Revision);

            if (admission.IsAdmitted == false)
            {
                return PlayerMovementResolutionOutcome.Rejected(MapRejectionReason(admission.RejectionReason), CreateNoChange());
            }

            if (_arenaBounds.Contains(resolution.AcceptedPosition, _player.CollisionRadius) == false)
            {
                _interactionLedger.AbandonPending();

                return PlayerMovementResolutionOutcome.Rejected(PlayerMovementResolutionRejectionReason.AcceptedPositionOutsideArena, CreateNoChange());
            }

            _interactionLedger.Complete(resolution.Correlation.InteractionId);

            if (_player.Position == resolution.AcceptedPosition)
            {
                return PlayerMovementResolutionOutcome.AcceptedWithoutStateChange(CreateNoChange());
            }

            AggregateRevision nextRevision = Revision.Next();
            _player.MoveTo(resolution.AcceptedPosition);
            Revision = nextRevision;

            return PlayerMovementResolutionOutcome.Applied(CreateStateChange());
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

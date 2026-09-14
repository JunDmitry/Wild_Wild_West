using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Contracts;

namespace Game.Arena.Domain.Interactions
{
    internal sealed class PendingInteractionLedger
    {
        private readonly ArenaRunId _owner;

        private InteractionId _lastIssued;
        private bool _hasPending;
        private InteractionCorrelation _correlation;
        private InteractionKind _kind;

        public PendingInteractionLedger(ArenaRunId owner)
        {
            if (owner.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(owner));
            }

            _owner = owner;
            _lastIssued = InteractionId.None;
            _hasPending = false;
        }

        public bool HasPending => _hasPending;
        public InteractionId LastIssued => _lastIssued;

        public InteractionCorrelation Open(
            InteractionKind kind,
            AggregateRevision currentRevision)
        {
            if (_hasPending)
            {
                throw new InvalidOperationException("An interaction is already pending.");
            }

            _lastIssued = _lastIssued.Next();
            _correlation = new InteractionCorrelation(
                _owner,
                _lastIssued,
                currentRevision);
            _kind = kind;
            _hasPending = true;

            return _correlation;
        }

        public InteractionAdmission Admit(
            IInteractionResolution resolution,
            AggregateRevision currentRevision)
        {
            if (resolution == null)
            {
                throw new ArgumentNullException(nameof(resolution));
            }

            InteractionCorrelation correlation = resolution.Correlation;

            if (correlation.ArenaRunId != _owner)
            {
                return InteractionAdmission.Rejected(InteractionRejectionReason.ForeignArenaRun);
            }

            bool isUnknown = correlation.InteractionId.IsNone || correlation.InteractionId.Value > _lastIssued.Value;

            if (isUnknown)
            {
                return InteractionAdmission.Rejected(InteractionRejectionReason.UnknownInteraction);
            }

            bool isPendingOne = _hasPending && correlation.InteractionId == _correlation.InteractionId;

            if (isPendingOne == false)
            {
                return InteractionAdmission.Rejected(InteractionRejectionReason.InteractionClosed);
            }

            bool isStale = correlation.AggregateRevision != _correlation.AggregateRevision || currentRevision != _correlation.AggregateRevision;

            if (isStale)
            {
                return InteractionAdmission.Rejected(InteractionRejectionReason.StaleRevision);
            }

            if (resolution.Kind != _kind)
            {
                return InteractionAdmission.Rejected(InteractionRejectionReason.KindMismatch);
            }

            return InteractionAdmission.Admitted;
        }

        public void Complete(InteractionId interactionId)
        {
            bool isPendingOne = _hasPending && interactionId == _correlation.InteractionId;

            if (isPendingOne == false)
            {
                throw new InvalidOperationException("The interaction is not pending.");
            }

            _hasPending = false;
        }

        public bool AbandonPending()
        {
            if (_hasPending == false)
            {
                return false;
            }

            _hasPending = false;

            return true;
        }
    }
}

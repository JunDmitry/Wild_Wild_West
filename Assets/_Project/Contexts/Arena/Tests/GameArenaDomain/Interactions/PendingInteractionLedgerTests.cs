using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Contracts;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Interactions
{
    [TestFixture]
    public sealed class PendingInteractionLedgerTests
    {
        private static readonly ArenaRunId s_owner = ArenaRunId.FromValue(7UL);

        [Test]
        public void OpenAllocatesIncreasingInteractionIds()
        {
            PendingInteractionLedger ledger = new(s_owner);

            InteractionCorrelation first = ledger.Open(InteractionKind.PlayerMovement, AggregateRevision.Initial);
            ledger.Complete(first.InteractionId);

            InteractionCorrelation second = ledger.Open(InteractionKind.PlayerAttack, AggregateRevision.Initial);

            Assert.That(first.InteractionId.Value, Is.EqualTo(1UL));
            Assert.That(second.InteractionId.Value, Is.EqualTo(2UL));
            Assert.That(first.ArenaRunId, Is.EqualTo(s_owner));
        }

        [Test]
        public void OpenWhilePendingThrows()
        {
            PendingInteractionLedger ledger = new(s_owner);
            ledger.Open(InteractionKind.PlayerMovement, AggregateRevision.Initial);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    ledger.Open(InteractionKind.PlayerAttack, AggregateRevision.Initial);
                });
        }

        [Test]
        public void MatchingResolutionIsAdmitted()
        {
            PendingInteractionLedger ledger = new(s_owner);
            InteractionCorrelation correlation = ledger.Open(
                InteractionKind.PlayerMovement,
                AggregateRevision.Initial);

            InteractionAdmission admission = ledger.Admit(new TestResolution(correlation, InteractionKind.PlayerMovement), AggregateRevision.Initial);

            Assert.That(admission.IsAdmitted, Is.True);
        }

        [Test]
        public void ResolutionForAnotherArenaRunIsRejected()
        {
            PendingInteractionLedger ledger = new(s_owner);
            InteractionCorrelation correlation = ledger.Open(InteractionKind.PlayerMovement, AggregateRevision.Initial);

            InteractionCorrelation foreign = new(ArenaRunId.FromValue(99UL), correlation.InteractionId, correlation.AggregateRevision);
            InteractionAdmission admission = ledger.Admit(new TestResolution(foreign, InteractionKind.PlayerMovement), AggregateRevision.Initial);

            Assert.That(admission.RejectionReason, Is.EqualTo(InteractionRejectionReason.ForeignArenaRun));
        }

        [Test]
        public void UnknownInteractionResolutionIsRejected()
        {
            PendingInteractionLedger ledger = new(s_owner);
            ledger.Open(InteractionKind.PlayerMovement, AggregateRevision.Initial);

            InteractionCorrelation unknown = new(s_owner, InteractionId.None.Next().Next(), AggregateRevision.Initial);
            InteractionAdmission admission = ledger.Admit(new TestResolution(unknown, InteractionKind.PlayerMovement), AggregateRevision.Initial);

            Assert.That(admission.RejectionReason, Is.EqualTo(InteractionRejectionReason.UnknownInteraction));
        }

        [Test]
        public void DuplicateInteractionResolutionIsRejected()
        {
            PendingInteractionLedger ledger = new(s_owner);
            InteractionCorrelation correlation = ledger.Open(InteractionKind.PlayerMovement, AggregateRevision.Initial);
            TestResolution resolution = new(correlation, InteractionKind.PlayerMovement);

            ledger.Admit(resolution, AggregateRevision.Initial);
            ledger.Complete(correlation.InteractionId);

            InteractionAdmission second = ledger.Admit(resolution, AggregateRevision.Initial.Next());

            Assert.That(second.RejectionReason, Is.EqualTo(InteractionRejectionReason.InteractionClosed));
        }

        [Test]
        public void StaleInteractionResolutionIsRejected()
        {
            PendingInteractionLedger ledger = new(s_owner);
            InteractionCorrelation correlation = ledger.Open(InteractionKind.PlayerMovement, AggregateRevision.Initial);

            InteractionAdmission admission = ledger.Admit(new TestResolution(correlation, InteractionKind.PlayerMovement), AggregateRevision.Initial.Next());

            Assert.That(admission.RejectionReason, Is.EqualTo(InteractionRejectionReason.StaleRevision));
        }

        [Test]
        public void ResolutionCarryingOldRevisionIsRejected()
        {
            PendingInteractionLedger ledger = new(s_owner);
            AggregateRevision current = AggregateRevision.Initial.Next();
            InteractionCorrelation correlation = ledger.Open(InteractionKind.PlayerMovement, current);

            InteractionCorrelation old = new(s_owner, correlation.InteractionId, AggregateRevision.Initial);

            InteractionAdmission admission = ledger.Admit(new TestResolution(old, InteractionKind.PlayerMovement), current);

            Assert.That(admission.RejectionReason, Is.EqualTo(InteractionRejectionReason.StaleRevision));
        }

        [Test]
        public void MismatchedInteractionResolutionIsRejected()
        {
            PendingInteractionLedger ledger = new(s_owner);
            InteractionCorrelation correlation = ledger.Open(InteractionKind.PlayerMovement, AggregateRevision.Initial);

            InteractionAdmission admission = ledger.Admit(new TestResolution(correlation, InteractionKind.PlayerAttack), AggregateRevision.Initial);

            Assert.That(admission.RejectionReason, Is.EqualTo(InteractionRejectionReason.KindMismatch));
        }

        [Test]
        public void CompleteOnNonPendingInteractionThrows()
        {
            PendingInteractionLedger ledger = new(s_owner);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    ledger.Complete(InteractionId.None.Next());
                });
        }

        [Test]
        public void CancelledInteractionResolutionIsRejected()
        {
            PendingInteractionLedger ledger = new(s_owner);
            InteractionCorrelation correlation = ledger.Open(
                InteractionKind.PlayerMovement,
                AggregateRevision.Initial);

            bool cancelled = ledger.Cancel(correlation);
            InteractionAdmission admission = ledger.Admit(new TestResolution(correlation, InteractionKind.PlayerMovement), AggregateRevision.Initial);

            Assert.That(cancelled, Is.True);
            Assert.That(ledger.HasPending, Is.False);
            Assert.That(admission.RejectionReason, Is.EqualTo(InteractionRejectionReason.InteractionClosed));
        }

        [Test]
        public void CancelWithMismatchedCorrelationKeepsPending()
        {
            PendingInteractionLedger ledger = new(s_owner);
            ledger.Open(InteractionKind.PlayerMovement, AggregateRevision.Initial);

            InteractionCorrelation foreign = new(
                s_owner,
                InteractionId.None.Next().Next(),
                AggregateRevision.Initial);

            bool cancelled = ledger.Cancel(foreign);

            Assert.That(cancelled, Is.False);
            Assert.That(ledger.HasPending, Is.True);
        }

        private sealed class TestResolution : IInteractionResolution
        {
            public TestResolution(InteractionCorrelation correlation, InteractionKind kind)
            {
                Correlation = correlation;
                Kind = kind;
            }

            public InteractionCorrelation Correlation { get; }

            public InteractionKind Kind { get; }
        }
    }
}

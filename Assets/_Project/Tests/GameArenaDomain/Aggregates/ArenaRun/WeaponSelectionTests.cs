using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class WeaponSelectionTests
    {
        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void NewRunSelectsRangedWeapon()
        {
            ArenaRun run = CreateRun();

            Assert.That(run.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
        }

        [Test]
        public void SwitchWeaponChangesSelectedWeapon()
        {
            ArenaRun run = CreateRun();

            WeaponSwitchOutcome first = run.SwitchWeapon();
            WeaponSwitchOutcome second = run.SwitchWeapon();

            Assert.That(first.Status, Is.EqualTo(WeaponSwitchStatus.Switched));
            Assert.That(first.SelectedWeapon, Is.EqualTo(WeaponKind.Melee));
            Assert.That(second.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
            Assert.That(run.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
        }

        [Test]
        public void SwitchWeaponAdvancesRevisionOnce()
        {
            ArenaRun run = CreateRun();

            WeaponSwitchOutcome outcome = run.SwitchWeapon();

            Assert.That(run.Revision.Value, Is.EqualTo(1UL));
            Assert.That(outcome.Change.Revision, Is.EqualTo(run.Revision));
            Assert.That(outcome.Change.HasStateChange, Is.True);
        }

        [Test]
        public void SwitchWeaponDoesNotProduceDomainEvents()
        {
            ArenaRun run = CreateRun();

            WeaponSwitchOutcome outcome = run.SwitchWeapon();

            Assert.That(outcome.Change.DomainEvents, Is.Empty);
        }

        [Test]
        public void SwitchWeaponIsRejectedWhileInteractionIsPending()
        {
            ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));
            run.RequestPlayerMovement(input, new GameDuration(1d));

            WeaponSwitchOutcome outcome = run.SwitchWeapon();

            Assert.That(outcome.Status, Is.EqualTo(WeaponSwitchStatus.InteractionPending));
            Assert.That(outcome.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
            Assert.That(run.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(outcome.Change.HasStateChange, Is.False);
        }

        [Test]
        public void SwitchWeaponIsAllowedAfterPendingInteractionIsResolved()
        {
            ArenaRun run = CreateRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));
            PlayerMovementRequestOutcome requestOutcome = run.RequestPlayerMovement(
                input,
                new GameDuration(1d));

            run.ApplyPlayerMovement(
                new PlayerMovementResolution(
                    requestOutcome.Request.Correlation,
                    requestOutcome.Request.RequestedPosition));

            WeaponSwitchOutcome outcome = run.SwitchWeapon();

            Assert.That(outcome.Status, Is.EqualTo(WeaponSwitchStatus.Switched));
            Assert.That(run.Revision.Value, Is.EqualTo(2UL));
        }

        [Test]
        public void SwitchWeaponDoesNotAffectPlayerHealth()
        {
            ArenaRun run = CreateRun();

            run.SwitchWeapon();

            Assert.That(run.PlayerHealth, Is.EqualTo(Health.Full(100)));
        }

        private ArenaRun CreateRun()
        {
            return _kit.StartRun();
        }
    }

    [TestFixture]
    public sealed class GameTimeTests
    {
        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void NewRunStartsAtTimeZeroWithReadyWeapons()
        {
            ArenaRun run = _kit.StartRun();

            Assert.That(run.CurrentTime, Is.EqualTo(new GameTimePoint(0d)));
            Assert.That(run.IsSelectedWeaponReady, Is.True);
            Assert.That(run.WeaponReadyAt(WeaponKind.Melee), Is.EqualTo(new GameTimePoint(0d)));
        }

        [Test]
        public void AdvanceTimeMovesCurrentTimeAndAdvancesRevisionOnce()
        {
            ArenaRun run = _kit.StartRun();

            TimeAdvanceOutcome outcome = run.AdvanceTime(new GameDuration(0.5d));

            Assert.That(outcome.IsAdvanced, Is.True);
            Assert.That(run.CurrentTime.Seconds, Is.EqualTo(0.5d));
            Assert.That(run.Revision.Value, Is.EqualTo(1UL));
            Assert.That(outcome.Change.HasStateChange, Is.True);
            Assert.That(outcome.Change.DomainEvents, Is.Empty);
        }

        [Test]
        public void AdvanceTimeAccumulates()
        {
            ArenaRun run = _kit.StartRun();

            run.AdvanceTime(new GameDuration(0.25d));
            run.AdvanceTime(new GameDuration(0.25d));

            Assert.That(run.CurrentTime.Seconds, Is.EqualTo(0.5d));
            Assert.That(run.Revision.Value, Is.EqualTo(2UL));
        }

        [Test]
        public void AdvanceTimeRejectsZeroDuration()
        {
            ArenaRun run = _kit.StartRun();

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    run.AdvanceTime(new GameDuration(0d));
                });
        }

        [Test]
        public void AdvanceTimeIsRejectedWhileInteractionIsPending()
        {
            ArenaRun run = _kit.StartRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));
            run.RequestPlayerMovement(input, new GameDuration(1d));

            TimeAdvanceOutcome outcome = run.AdvanceTime(new GameDuration(1d));

            Assert.That(outcome.Status, Is.EqualTo(TimeAdvanceStatus.InteractionPending));
            Assert.That(run.CurrentTime, Is.EqualTo(new GameTimePoint(0d)));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
            Assert.That(outcome.Change.HasStateChange, Is.False);
        }

        [Test]
        public void MovementRequestedAfterTimeAdvanceCarriesNewRevision()
        {
            ArenaRun run = _kit.StartRun();
            run.AdvanceTime(new GameDuration(1d));
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            PlayerMovementRequestOutcome outcome = run.RequestPlayerMovement(input, new GameDuration(1d));

            Assert.That(outcome.Request.Correlation.AggregateRevision.Value, Is.EqualTo(1UL));
        }
    }
}

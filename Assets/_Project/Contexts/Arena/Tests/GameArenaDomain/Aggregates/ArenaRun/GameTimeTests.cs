using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
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

        [Test]
        public void SwitchWeaponIsRejectedWhileAttackIsPending()
        {
            ArenaRun run = _kit.StartRun();

            run.StartPlayerAttack();
            WeaponSwitchOutcome outcome = run.SwitchWeapon();

            Assert.That(outcome.Status, Is.EqualTo(WeaponSwitchStatus.AttackPending));
            Assert.That(outcome.Change.HasStateChange, Is.False);
        }
    }
}

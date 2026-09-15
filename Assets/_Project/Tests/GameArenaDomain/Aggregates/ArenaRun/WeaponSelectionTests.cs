using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;
using Game.Arena.Domain.Vitality;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class WeaponSelectionTests
    {
        private ArenaBounds _arenaBounds;
        private ArenaRunFactory _factory;
        private CollisionRadius _playerRadius;
        private MovementSpeed _playerSpeed;

        [SetUp]
        public void SetUp()
        {
            _factory = new ArenaRunFactory();
            _arenaBounds = new ArenaBounds(-20f, 20f, -20f, 20f, 0f);
            _playerRadius = CollisionRadius.FromValue(0.5f);
            _playerSpeed = MovementSpeed.FromUnitsPerSecond(5f);
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
            return _factory.Start(
                ArenaRunId.FromValue(1UL),
                PlayerId.FromValue(2UL),
                Position3D.Zero,
                Health.Full(100),
                _playerSpeed,
                _playerRadius,
                _arenaBounds);
        }
    }
}

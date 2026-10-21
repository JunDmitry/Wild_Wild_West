using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class PlayerAttackStartTests
    {
        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void StartingPlayerAttackDoesNotDamageEnemies()
        {
            ArenaRun run = _kit.StartRun();
            SpawnOneRegular(run);

            PlayerAttackStartOutcome outcome = run.StartPlayerAttack();

            Assert.That(outcome.IsStarted, Is.True);
            Assert.That(run.ActiveEnemyCount, Is.EqualTo(1));
        }

        [Test]
        public void StartingPlayerAttackAdvancesRevisionOnce()
        {
            ArenaRun run = _kit.StartRun();

            PlayerAttackStartOutcome outcome = run.StartPlayerAttack();

            Assert.That(outcome.IsStarted, Is.True);
            Assert.That(run.Revision.Value, Is.EqualTo(1UL));
            Assert.That(outcome.Change.Revision, Is.EqualTo(run.Revision));
            Assert.That(outcome.Change.HasStateChange, Is.True);
        }

        [Test]
        public void StartingPlayerAttackProducesPlayerAttackStartedEvent()
        {
            ArenaRun run = _kit.StartRun();

            PlayerAttackStartOutcome outcome = run.StartPlayerAttack();

            Assert.That(outcome.Change.DomainEvents.Count, Is.EqualTo(1));

            PlayerAttackStarted attackStarted = (PlayerAttackStarted)outcome.Change.DomainEvents[0];

            Assert.That(attackStarted.ArenaRunId, Is.EqualTo(run.Id));
            Assert.That(attackStarted.AggregateRevision, Is.EqualTo(run.Revision));
            Assert.That(attackStarted.AttackId, Is.EqualTo(outcome.Attack.Id));
            Assert.That(attackStarted.WeaponKind, Is.EqualTo(WeaponKind.Ranged));
            Assert.That(attackStarted.StartedAt, Is.EqualTo(new GameTimePoint(0d)));
            Assert.That(attackStarted.ImpactAt, Is.EqualTo(new GameTimePoint(0d)));
        }

        [Test]
        public void RangedAttackUsesCooldownAtStart()
        {
            ArenaRun run = _kit.StartRun();

            run.StartPlayerAttack();

            Assert.That(run.WeaponReadyAt(WeaponKind.Ranged), Is.EqualTo(new GameTimePoint(0.4d)));
        }

        [Test]
        public void StartingSecondAttackWhileFirstPendingIsRejected()
        {
            ArenaRun run = _kit.StartRun();

            run.StartPlayerAttack();
            PlayerAttackStartOutcome second = run.StartPlayerAttack();

            Assert.That(second.Status, Is.EqualTo(PlayerAttackStartStatus.AttackAlreadyPending));
            Assert.That(run.Revision.Value, Is.EqualTo(1UL));
        }

        [Test]
        public void WeaponNotReadyRejectsAttackAfterPendingAttackIsNotEnough()
        {
            ArenaRun run = _kit.StartRun();

            run.StartPlayerAttack();
            run.AdvanceTime(new GameDuration(0.1d));

            PlayerAttackStartOutcome second = run.StartPlayerAttack();

            Assert.That(second.Status, Is.EqualTo(PlayerAttackStartStatus.AttackAlreadyPending));
        }

        [Test]
        public void StartingAttackWhileInteractionPendingIsRejected()
        {
            ArenaRun run = _kit.StartRun();
            MovementInput input = MovementInput.FromVector(new Displacement3D(1f, 0f, 0f));

            run.RequestPlayerMovement(input, new GameDuration(1d));
            PlayerAttackStartOutcome outcome = run.StartPlayerAttack();

            Assert.That(outcome.Status, Is.EqualTo(PlayerAttackStartStatus.InteractionPending));
            Assert.That(run.Revision, Is.EqualTo(AggregateRevision.Initial));
        }

        [Test]
        public void SwitchWeaponIsRejectedWhileAttackPending()
        {
            ArenaRun run = _kit.StartRun();

            run.StartPlayerAttack();
            WeaponSwitchOutcome outcome = run.SwitchWeapon();

            Assert.That(outcome.Status, Is.EqualTo(WeaponSwitchStatus.AttackPending));
            Assert.That(run.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
        }

        [Test]
        public void MeleeAttackHasWindup()
        {
            ArenaRun run = _kit.StartRun();

            run.SwitchWeapon();
            PlayerAttackStartOutcome outcome = run.StartPlayerAttack();

            Assert.That(outcome.Attack.WeaponKind, Is.EqualTo(WeaponKind.Melee));
            Assert.That(outcome.Attack.StartedAt, Is.EqualTo(new GameTimePoint(0d)));
            Assert.That(outcome.Attack.ImpactAt, Is.EqualTo(new GameTimePoint(0.3d)));
        }

        private void SpawnOneRegular(ArenaRun run)
        {
            Domain.Interactions.Spawn.EnemySpawnRequest request = run.RequestEnemySpawn().Request;

            run.ApplyEnemySpawn(
                new Game.Arena.Domain.Interactions.Spawn.EnemySpawnResolution(
                    request.Correlation,
                    Game.Arena.Domain.Identity.EnemyId.FromValue(10UL),
                    new Position3D(22f, 0f, 0f)));
        }
    }
}

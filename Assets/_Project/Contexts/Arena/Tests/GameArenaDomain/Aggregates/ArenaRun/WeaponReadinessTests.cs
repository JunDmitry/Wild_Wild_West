using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Attack;
using Game.Arena.Domain.Tests.Support;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Aggregates._ArenaRun
{
    [TestFixture]
    public sealed class WeaponReadinessTests
    {
        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void WeaponReadinessSurvivesSwitchAwayAndBack()
        {
            ArenaRun run = _kit.StartRun();

            run.StartPlayerAttack();
            run.AdvanceTime(new GameDuration(0.05d));

            PlayerAttackImpactRequest impactRequest =
                run.RequestPlayerAttackImpact(Direction3D.Right).Request;

            run.ApplyPlayerAttackImpact(
                new PlayerAttackImpactResolution(impactRequest.Correlation, Array.Empty<EnemyId>()));

            GameTimePoint rangedReadyBefore = run.WeaponReadyAt(WeaponKind.Ranged);
            GameTimePoint meleeReadyBefore = run.WeaponReadyAt(WeaponKind.Melee);

            run.SwitchWeapon();
            run.SwitchWeapon();

            Assert.That(run.SelectedWeapon, Is.EqualTo(WeaponKind.Ranged));
            Assert.That(run.WeaponReadyAt(WeaponKind.Ranged), Is.EqualTo(rangedReadyBefore));
            Assert.That(run.IsSelectedWeaponReady, Is.False);
            Assert.That(run.CurrentTime, Is.LessThan(rangedReadyBefore));
            Assert.That(run.WeaponReadyAt(WeaponKind.Melee), Is.EqualTo(meleeReadyBefore));
            Assert.That(run.WeaponReadyAt(WeaponKind.Melee), Is.LessThanOrEqualTo(run.CurrentTime));
        }
    }
}

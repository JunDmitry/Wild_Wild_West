using System;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests._Combat
{
    [TestFixture]
    public sealed class PendingAttackTests
    {
        [Test]
        public void PendingPlayerAttackRejectsNoneAttackId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    PendingPlayerAttack unused = new PendingPlayerAttack(
                        AttackId.None,
                        PlayerId.FromValue(1UL),
                        WeaponKind.Ranged,
                        new GameTimePoint(0d),
                        new GameTimePoint(0d));
                });
        }

        [Test]
        public void PendingPlayerAttackRejectsNonePlayerId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    PendingPlayerAttack unused = new PendingPlayerAttack(
                        AttackId.None.Next(),
                        PlayerId.None,
                        WeaponKind.Ranged,
                        new GameTimePoint(0d),
                        new GameTimePoint(0d));
                });
        }

        [Test]
        public void PendingEnemyAttackRejectsNoneEnemyId()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    PendingEnemyAttack unused = new PendingEnemyAttack(
                        AttackId.None.Next(),
                        EnemyId.None,
                        new GameTimePoint(0d),
                        new GameTimePoint(0d));
                });
        }

        [Test]
        public void PendingAttacksRejectImpactBeforeStart()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    PendingPlayerAttack unused = new PendingPlayerAttack(
                        AttackId.None.Next(),
                        PlayerId.FromValue(1UL),
                        WeaponKind.Melee,
                        new GameTimePoint(2d),
                        new GameTimePoint(1d));
                });

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    PendingEnemyAttack unused = new PendingEnemyAttack(
                        AttackId.None.Next(),
                        EnemyId.FromValue(2UL),
                        new GameTimePoint(2d),
                        new GameTimePoint(1d));
                });
        }

        [Test]
        public void PendingPlayerAttackBecomesReadyAtImpactTime()
        {
            PendingPlayerAttack attack = new PendingPlayerAttack(
                AttackId.None.Next(),
                PlayerId.FromValue(1UL),
                WeaponKind.Ranged,
                new GameTimePoint(1d),
                new GameTimePoint(2d));

            Assert.That(attack.IsReadyToImpact(new GameTimePoint(1.9d)), Is.False);
            Assert.That(attack.IsReadyToImpact(new GameTimePoint(2d)), Is.True);
        }

        [Test]
        public void PendingEnemyAttackBecomesReadyAtImpactTime()
        {
            PendingEnemyAttack attack = new PendingEnemyAttack(
                AttackId.None.Next(),
                EnemyId.FromValue(2UL),
                new GameTimePoint(1d),
                new GameTimePoint(2d));

            Assert.That(attack.IsReadyToImpact(new GameTimePoint(1.9d)), Is.False);
            Assert.That(attack.IsReadyToImpact(new GameTimePoint(2d)), Is.True);
        }
    }
}

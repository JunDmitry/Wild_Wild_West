using System;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Identity
{
    [TestFixture]
    public sealed class EnemyIdTests
    {
        [Test]
        public void NoneIsZeroValueAndIsNone()
        {
            EnemyId id = EnemyId.None;

            Assert.That(id.Value, Is.EqualTo(0UL));
            Assert.That(id.IsNone, Is.True);
        }

        [Test]
        public void FromValueRejectsZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    EnemyId.FromValue(0UL);
                });
        }

        [Test]
        public void FromValueCreatesNonNoneIdentity()
        {
            EnemyId id = EnemyId.FromValue(7UL);

            Assert.That(id.Value, Is.EqualTo(7UL));
            Assert.That(id.IsNone, Is.False);
        }

        [Test]
        public void EqualityIsBasedOnValue()
        {
            EnemyId left = EnemyId.FromValue(3UL);
            EnemyId right = EnemyId.FromValue(3UL);
            EnemyId other = EnemyId.FromValue(4UL);

            Assert.That(left.Equals(right), Is.True);
            Assert.That(left == right, Is.True);
            Assert.That(left != other, Is.True);
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void DefaultValueEqualsNone()
        {
            EnemyId defaultId = default;

            Assert.That(defaultId, Is.EqualTo(EnemyId.None));
        }
    }
}

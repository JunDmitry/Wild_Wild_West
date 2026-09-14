using System;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Identity
{
    [TestFixture]
    public sealed class PlayerIdTests
    {
        [Test]
        public void NoneIsZeroValueAndIsNone()
        {
            PlayerId id = PlayerId.None;

            Assert.That(id.Value, Is.EqualTo(0UL));
            Assert.That(id.IsNone, Is.True);
        }

        [Test]
        public void FromValueRejectsZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    PlayerId.FromValue(0UL);
                });
        }

        [Test]
        public void FromValueCreatesNonNoneIdentity()
        {
            PlayerId id = PlayerId.FromValue(7UL);

            Assert.That(id.Value, Is.EqualTo(7UL));
            Assert.That(id.IsNone, Is.False);
        }

        [Test]
        public void EqualityIsBasedOnValue()
        {
            PlayerId left = PlayerId.FromValue(3UL);
            PlayerId right = PlayerId.FromValue(3UL);
            PlayerId other = PlayerId.FromValue(4UL);

            Assert.That(left.Equals(right), Is.True);
            Assert.That(left == right, Is.True);
            Assert.That(left != other, Is.True);
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void DefaultValueEqualsNone()
        {
            PlayerId defaultId = default;

            Assert.That(defaultId, Is.EqualTo(PlayerId.None));
        }
    }
}

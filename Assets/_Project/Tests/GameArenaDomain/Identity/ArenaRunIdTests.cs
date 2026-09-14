using System;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Identity
{
    [TestFixture]
    public sealed class ArenaRunIdTests
    {
        [Test]
        public void NoneIsZeroValueAndIsNone()
        {
            ArenaRunId id = ArenaRunId.None;

            Assert.That(id.Value, Is.EqualTo(0UL));
            Assert.That(id.IsNone, Is.True);
        }

        [Test]
        public void FromValueRejectsZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    ArenaRunId.FromValue(0UL);
                });
        }

        [Test]
        public void FromValueCreatesNonNoneIdentity()
        {
            ArenaRunId id = ArenaRunId.FromValue(7UL);

            Assert.That(id.Value, Is.EqualTo(7UL));
            Assert.That(id.IsNone, Is.False);
        }

        [Test]
        public void EqualityIsBasedOnValue()
        {
            ArenaRunId left = ArenaRunId.FromValue(3UL);
            ArenaRunId right = ArenaRunId.FromValue(3UL);
            ArenaRunId other = ArenaRunId.FromValue(4UL);

            Assert.That(left.Equals(right), Is.True);
            Assert.That(left == right, Is.True);
            Assert.That(left != other, Is.True);
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        }

        [Test]
        public void DefaultValueEqualsNone()
        {
            ArenaRunId defaultId = default;

            Assert.That(defaultId, Is.EqualTo(ArenaRunId.None));
        }
    }
}

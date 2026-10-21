using Game.Arena.Domain.Interactions;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Interactions
{
    [TestFixture]
    public sealed class InteractionIdTests
    {
        [Test]
        public void NoneIsZeroAndIsNone()
        {
            Assert.That(InteractionId.None.Value, Is.EqualTo(0UL));
            Assert.That(InteractionId.None.IsNone, Is.True);
        }

        [Test]
        public void NextFromNoneStartsAtOne()
        {
            InteractionId first = InteractionId.None.Next();

            Assert.That(first.Value, Is.EqualTo(1UL));
            Assert.That(first.IsNone, Is.False);
        }

        [Test]
        public void SequentialNextValuesAreUnique()
        {
            InteractionId first = InteractionId.None.Next();
            InteractionId second = first.Next();

            Assert.That(first, Is.Not.EqualTo(second));
        }
    }
}

using Game.Arena.Domain.Concurrency;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Concurrency
{
    [TestFixture]
    public sealed class AggregateRevisionTests
    {
        [Test]
        public void InitialHasZeroValue()
        {
            Assert.That(AggregateRevision.Initial.Value, Is.EqualTo(0UL));
        }

        [Test]
        public void NextIncrementsValue()
        {
            AggregateRevision next = AggregateRevision.Initial.Next();

            Assert.That(next.Value, Is.EqualTo(1UL));
        }

        [Test]
        public void ComparisonOrdersRevisions()
        {
            AggregateRevision first = AggregateRevision.Initial;
            AggregateRevision second = first.Next();

            Assert.That(first < second, Is.True);
            Assert.That(second > first, Is.True);
#pragma warning disable CS1718
            Assert.That(first <= first, Is.True);
#pragma warning restore CS1718
        }

        [Test]
        public void EqualRevisionsAreEqual()
        {
            AggregateRevision first = AggregateRevision.Initial.Next();
            AggregateRevision second = AggregateRevision.Initial.Next();

            Assert.That(first == second, Is.True);
            Assert.That(first.Equals(second), Is.True);
        }
    }
}

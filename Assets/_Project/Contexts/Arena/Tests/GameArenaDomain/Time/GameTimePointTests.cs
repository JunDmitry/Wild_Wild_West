using System;
using Game.Arena.Domain.Time;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Time
{
    [TestFixture]
    public sealed class GameTimePointTests
    {
        [Test]
        public void ConstructorRejectsNegativeSeconds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    GameTimePoint unused = new(-1d);
                });
        }

        [Test]
        public void AddingDurationAdvancesTimePoint()
        {
            GameTimePoint start = new(10d);
            GameDuration duration = new(5d);

            GameTimePoint result = start + duration;

            Assert.That(result.Seconds, Is.EqualTo(15d));
        }

        [Test]
        public void SubtractingEarlierPointProducesDuration()
        {
            GameTimePoint earlier = new(4d);
            GameTimePoint later = new(10d);

            GameDuration elapsed = later - earlier;

            Assert.That(elapsed.Seconds, Is.EqualTo(6d));
        }

        [Test]
        public void SubtractingLaterPointThrows()
        {
            GameTimePoint earlier = new(4d);
            GameTimePoint later = new(10d);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    GameDuration elapsed = earlier - later;
                });
        }

        [Test]
        public void ComparisonOrdersTimePoints()
        {
            GameTimePoint earlier = new(1d);
            GameTimePoint later = new(2d);

            Assert.That(earlier < later, Is.True);
            Assert.That(later > earlier, Is.True);
        }
    }
}

using System;
using System.Threading.Tasks;
using Game.Arena.Application.Identity;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Identity
{
    [TestFixture]
    public sealed class TypedIdentitySourceTests
    {
        [Test]
        public void EachIdentitySourceStartsFromOneIndependently()
        {
            IArenaRunIdSource arenaRunSource = new MonotonicArenaRunIdSource();
            IPlayerIdSource playerSource = new MonotonicPlayerIdSource();
            IEnemyIdSource enemySource = new MonotonicEnemyIdSource();

            Assert.That(arenaRunSource.Allocate().Value, Is.EqualTo(1UL));
            Assert.That(playerSource.Allocate().Value, Is.EqualTo(1UL));
            Assert.That(enemySource.Allocate().Value, Is.EqualTo(1UL));
        }

        [Test]
        public void ArenaRunSourceAllocatesMonotonically()
        {
            MonotonicArenaRunIdSource source = new();

            Assert.That(source.Allocate().Value, Is.EqualTo(1UL));
            Assert.That(source.Allocate().Value, Is.EqualTo(2UL));
            Assert.That(source.Allocate().Value, Is.EqualTo(3UL));
        }

        [Test]
        public void PlayerSourceAllocatesMonotonically()
        {
            MonotonicPlayerIdSource source = new();

            Assert.That(source.Allocate().Value, Is.EqualTo(1UL));
            Assert.That(source.Allocate().Value, Is.EqualTo(2UL));
            Assert.That(source.Allocate().Value, Is.EqualTo(3UL));
        }

        [Test]
        public void EnemySourceAllocatesMonotonically()
        {
            MonotonicEnemyIdSource source = new();

            Assert.That(source.Allocate().Value, Is.EqualTo(1UL));
            Assert.That(source.Allocate().Value, Is.EqualTo(2UL));
            Assert.That(source.Allocate().Value, Is.EqualTo(3UL));
        }

        [Test]
        public void ArenaRunSourceDoesNotShareCounterWithOtherSources()
        {
            MonotonicArenaRunIdSource arenaRunSource = new();

            MonotonicPlayerIdSource playerSource = new();

            arenaRunSource.Allocate();
            arenaRunSource.Allocate();
            playerSource.Allocate();

            Assert.That(arenaRunSource.Allocate().Value, Is.EqualTo(3UL));
            Assert.That(playerSource.Allocate().Value, Is.EqualTo(2UL));
        }

        [Test]
        public void AllocatedValuesAreNeverZero()
        {
            MonotonicArenaRunIdSource arenaRunSource = new();
            MonotonicPlayerIdSource playerSource = new();
            MonotonicEnemyIdSource enemySource = new();

            Assert.That(arenaRunSource.Allocate().IsNone, Is.False);
            Assert.That(playerSource.Allocate().IsNone, Is.False);
            Assert.That(enemySource.Allocate().IsNone, Is.False);
        }

        [Test]
        public void ArenaRunSourceThrowsAfterSequenceExhaustion()
        {
            MonotonicArenaRunIdSource source = new(ulong.MaxValue);

            Assert.That(source.Allocate().Value, Is.EqualTo(ulong.MaxValue));

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    source.Allocate();
                });

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    source.Allocate();
                });
        }

        [Test]
        public void PlayerSourceThrowsAfterSequenceExhaustion()
        {
            MonotonicPlayerIdSource source = new(ulong.MaxValue);

            Assert.That(source.Allocate().Value, Is.EqualTo(ulong.MaxValue));

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    source.Allocate();
                });
        }

        [Test]
        public void EnemySourceThrowsAfterSequenceExhaustion()
        {
            MonotonicEnemyIdSource source = new(ulong.MaxValue);

            Assert.That(source.Allocate().Value, Is.EqualTo(ulong.MaxValue));

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    source.Allocate();
                });
        }

        [Test]
        public void SourceIsSafeForConcurrentAllocation()
        {
            const int Count = 512;

            MonotonicEnemyIdSource source = new();

            ulong[] allocated = new ulong[Count];

            Parallel.For(
                0,
                Count,
                index =>
                {
                    allocated[index] = source.Allocate().Value;
                });

            Array.Sort(allocated);

            for (int index = 0; index < allocated.Length; index++)
            {
                Assert.That(allocated[index], Is.EqualTo((ulong)(index + 1)));
            }
        }
    }
}

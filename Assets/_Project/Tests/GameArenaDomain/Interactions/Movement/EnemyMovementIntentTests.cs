using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Movement;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Interactions.Movement
{
    [TestFixture]
    public sealed class EnemyMovementIntentTests
    {
        [Test]
        public void ConstructorRejectsNoneEnemyId()
        {
            PlanarMovementIntent intent = new PlanarMovementIntent(
                Position3D.Zero,
                Direction3D.Right,
                Distance.FromValue(1f));

            Assert.Throws<ArgumentException>(
                () =>
                {
                    EnemyMovementIntent unused = new EnemyMovementIntent(
                        EnemyId.None,
                        intent,
                        CollisionRadius.FromValue(0.5f));
                });
        }

        [Test]
        public void ConstructorRejectsInvalidIntent()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    EnemyMovementIntent unused = new EnemyMovementIntent(
                        EnemyId.FromValue(1UL),
                        default,
                        CollisionRadius.FromValue(0.5f));
                });
        }

        [Test]
        public void ConstructorRejectsInvalidCollisionRadius()
        {
            PlanarMovementIntent intent = new PlanarMovementIntent(
                Position3D.Zero,
                Direction3D.Right,
                Distance.FromValue(1f));

            Assert.Throws<ArgumentException>(
                () =>
                {
                    EnemyMovementIntent unused = new EnemyMovementIntent(
                        EnemyId.FromValue(1UL),
                        intent,
                        default);
                });
        }

        [Test]
        public void ConstructorStoresValues()
        {
            PlanarMovementIntent intent = new PlanarMovementIntent(
                Position3D.Zero,
                Direction3D.Right,
                Distance.FromValue(1f));

            EnemyMovementIntent movementIntent = new EnemyMovementIntent(
                EnemyId.FromValue(1UL),
                intent,
                CollisionRadius.FromValue(0.5f));

            Assert.That(movementIntent.EnemyId, Is.EqualTo(EnemyId.FromValue(1UL)));
            Assert.That(movementIntent.Intent, Is.EqualTo(intent));
            Assert.That(movementIntent.CollisionRadius, Is.EqualTo(CollisionRadius.FromValue(0.5f)));
        }

        [Test]
        public void EqualityIsBasedOnValues()
        {
            PlanarMovementIntent intent = new(
                Position3D.Zero,
                Direction3D.Right,
                Distance.FromValue(1f));

            EnemyMovementIntent left = new(
                EnemyId.FromValue(1UL),
                intent,
                CollisionRadius.FromValue(0.5f));

            EnemyMovementIntent right = new(
                EnemyId.FromValue(1UL),
                intent,
                CollisionRadius.FromValue(0.5f));

            Assert.That(left == right, Is.True);
            Assert.That(left.GetHashCode(), Is.EqualTo(right.GetHashCode()));
        }
    }

    [TestFixture]
    public sealed class EnemyMovementBatchResolutionTests
    {
        [Test]
        public void ConstructorRejectsNullEntries()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    EnemyMovementBatchResolution unused = new EnemyMovementBatchResolution(
                        CreateCorrelation(),
                        null);
                });
        }

        [Test]
        public void ConstructorCopiesEntryCollection()
        {
            EnemyMovementBatchResolutionEntry first = new(EnemyId.FromValue(1UL), Position3D.Zero);
            EnemyMovementBatchResolutionEntry second = new(EnemyId.FromValue(2UL), Position3D.Zero);
            EnemyMovementBatchResolutionEntry[] source =
            {
                first,
            };
            EnemyMovementBatchResolution resolution = new(CreateCorrelation(), source);

            source[0] = second;

            Assert.That(resolution.Entries.Count, Is.EqualTo(1));
            Assert.That(resolution.Entries[0].EnemyId, Is.EqualTo(EnemyId.FromValue(1UL)));
        }

        [Test]
        public void ResolutionKindIsEnemyMovementBatch()
        {
            EnemyMovementBatchResolution resolution = new(CreateCorrelation(), Array.Empty<EnemyMovementBatchResolutionEntry>());

            Assert.That(resolution.Kind, Is.EqualTo(InteractionKind.EnemyMovementBatch));
        }

        private InteractionCorrelation CreateCorrelation()
        {
            return new InteractionCorrelation(
                ArenaRunId.FromValue(1UL),
                InteractionId.None.Next(),
                AggregateRevision.Initial);
        }
    }
}

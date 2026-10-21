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
    public sealed class EnemyMovementBatchRequestTests
    {
        [Test]
        public void ConstructorRejectsNullIntents()
        {
            InteractionCorrelation correlation = CreateCorrelation();

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    EnemyMovementBatchRequest unused = new EnemyMovementBatchRequest(
                        correlation,
                        null);
                });
        }

        [Test]
        public void ConstructorRejectsEmptyIntents()
        {
            InteractionCorrelation correlation = CreateCorrelation();

            Assert.Throws<ArgumentException>(
                () =>
                {
                    EnemyMovementBatchRequest unused = new EnemyMovementBatchRequest(
                        correlation,
                        Array.Empty<EnemyMovementIntent>());
                });
        }

        [Test]
        public void ConstructorCopiesIntentCollection()
        {
            InteractionCorrelation correlation = CreateCorrelation();
            EnemyMovementIntent first = CreateIntent(1UL);
            EnemyMovementIntent second = CreateIntent(2UL);

            EnemyMovementIntent[] source =
            {
                first,
            };

            EnemyMovementBatchRequest request = new EnemyMovementBatchRequest(
                correlation,
                source);

            source[0] = second;

            Assert.That(request.Intents.Count, Is.EqualTo(1));
            Assert.That(request.Intents[0].EnemyId, Is.EqualTo(EnemyId.FromValue(1UL)));
        }

        [Test]
        public void RequestKindIsEnemyMovementBatch()
        {
            EnemyMovementBatchRequest request = new EnemyMovementBatchRequest(
                CreateCorrelation(),
                new[]
                {
                    CreateIntent(1UL),
                });

            Assert.That(request.Kind, Is.EqualTo(InteractionKind.EnemyMovementBatch));
        }

        [Test]
        public void ConstructorRejectsDuplicateEnemyIds()
        {
            InteractionCorrelation correlation = CreateCorrelation();
            EnemyMovementIntent intent = CreateIntent(1UL);

            Assert.Throws<ArgumentException>(
                () =>
                {
                    EnemyMovementBatchRequest unused = new EnemyMovementBatchRequest(
                        correlation,
                        new[]
                        {
                            intent,
                            intent,
                        });
                });
        }

        private InteractionCorrelation CreateCorrelation()
        {
            return new InteractionCorrelation(
                ArenaRunId.FromValue(1UL),
                InteractionId.None.Next(),
                AggregateRevision.Initial);
        }

        private EnemyMovementIntent CreateIntent(ulong enemyId)
        {
            PlanarMovementIntent intent = new PlanarMovementIntent(
                Position3D.Zero,
                Direction3D.Right,
                Distance.FromValue(1f));

            return new EnemyMovementIntent(
                EnemyId.FromValue(enemyId),
                intent,
                CollisionRadius.FromValue(0.5f));
        }
    }
}

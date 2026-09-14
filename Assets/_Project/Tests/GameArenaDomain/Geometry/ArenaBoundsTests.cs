using System;
using Game.Arena.Domain.Geometry;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Geometry
{
    [TestFixture]
    public sealed class ArenaBoundsTests
    {
        [Test]
        public void ConstructorRejectsInvalidHorizontalBounds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    ArenaBounds unused = new(10f, 10f, -10f, 10f, 0f);
                });
        }

        [Test]
        public void ContainsAccountsForCollisionRadius()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);

            CollisionRadius radius = CollisionRadius.FromValue(1f);

            Assert.That(bounds.Contains(new Position3D(9f, 0f, 0f), radius), Is.True);
            Assert.That(bounds.Contains(new Position3D(9.1f, 0f, 0f), radius), Is.False);
        }

        [Test]
        public void ContainsRejectsDifferentGroundHeight()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 0f);

            CollisionRadius radius = CollisionRadius.FromValue(1f);

            bool contains = bounds.Contains(new Position3D(0f, 1f, 0f), radius);

            Assert.That(contains, Is.False);
        }

        [Test]
        public void ClampAccountsForCollisionRadiusAndGroundHeight()
        {
            ArenaBounds bounds = new(-10f, 10f, -10f, 10f, 2f);

            CollisionRadius radius = CollisionRadius.FromValue(1f);

            Position3D result = bounds.Clamp(new Position3D(20f, 50f, -20f), radius);

            Assert.That(result, Is.EqualTo(new Position3D(9f, 2f, -9f)));
        }

        [Test]
        public void CanContainRejectsRadiusLargerThanArena()
        {
            ArenaBounds bounds = new(-1f, 1f, -1f, 1f, 0f);

            CollisionRadius radius = CollisionRadius.FromValue(2f);

            Assert.That(bounds.CanContain(radius), Is.False);
        }
    }
}

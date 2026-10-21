using System;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Tests.Support;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Configuration
{
    [TestFixture]
    public sealed class ArenaDefinitionTests
    {
        private ArenaRunTestKit _kit;

        [SetUp]
        public void SetUp()
        {
            _kit = new ArenaRunTestKit();
        }

        [Test]
        public void Constructor_IfZeroSpawnBand_ThrownOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new ArenaDefinition(new ArenaBounds(0, 1, 0, 1, 0), Distance.Zero);
            });
        }

        [Test]
        public void Constructor_IfCorrentSpawnBand_DoesNotThrown()
        {
            Assert.DoesNotThrow(() =>
            {
                _ = new ArenaDefinition(new ArenaBounds(0, 1, 0, 1, 0), Distance.FromValue(1));
            });
        }

        [Test]
        public void IsSpawnPosition_NotInGround_ReturnFalse()
        {
            ArenaDefinition arena = _kit.Arena;

            Assert.IsFalse(arena.IsSpawnPosition(new Position3D(0, 1, 0)));
        }

        [Test]
        public void IsSpawnPosition_IfInsideArena_ReturnFalse()
        {
            ArenaDefinition arena = _kit.Arena;
            Position3D[] positions = new[]
            {
                new Position3D(15, 0, 0),
                new Position3D(-15, 0, 0),
                new Position3D(0, 0, 15),
                new Position3D(0, 0, -15),
                new Position3D(15, 0, 15),
                new Position3D(-15, 0, -15),
                Position3D.Zero,
            };

            for (int i = 0; i < positions.Length; i++)
            {
                Assert.IsFalse(arena.IsSpawnPosition(positions[i]));
            }
        }

        [Test]
        public void IsSpawnPosition_IfOutsideBand_ReturnFalse()
        {
            ArenaDefinition arena = _kit.Arena;
            Position3D[] positions = new[]
{
                new Position3D(25.1f, 0, 0),
                new Position3D(-25.1f, 0, 0),
                new Position3D(0, 0, 25.1f),
                new Position3D(0, 0, -25.1f),
                new Position3D(25.1f, 0, 25.1f),
                new Position3D(-25.1f, 0, -25.1f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                Assert.IsFalse(arena.IsSpawnPosition(positions[i]));
            }
        }

        [Test]
        public void IsSpawnPosition_IfInsideBand_ReturnTrue()
        {
            ArenaDefinition arena = _kit.Arena;
            Position3D[] positions = new[]
{
                new Position3D(22.5f, 0, 0),
                new Position3D(-22.5f, 0, 0),
                new Position3D(0, 0, 22.5f),
                new Position3D(0, 0, -22.5f),
                new Position3D(22.5f, 0, 22.5f),
                new Position3D(-22.5f, 0, -22.5f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                Assert.IsTrue(arena.IsSpawnPosition(positions[i]));
            }
        }
    }
}

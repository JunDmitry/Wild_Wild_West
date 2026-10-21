using System;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Configuration
{
    [TestFixture]
    public sealed class WaveDefinitionTests
    {
        [Test]
        public void Constructor_IfNegativeRegularEnemyCount_ThrownOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _ = new WaveDefinition(WaveNumber.First, -1);
            });
        }
    }
}

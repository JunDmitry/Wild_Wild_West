using System;
using System.Collections.Generic;
using System.Linq;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Identity;
using NUnit.Framework;

namespace Game.Arena.Domain.Tests.Configuration
{
    [TestFixture]
    public sealed class WaveCatalogTests
    {
        [Test]
        public void Constructor_IfWavesNull_ThrownArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new WaveCatalog(null);
            });
        }

        [Test]
        public void Constructor_IfEmptyWaves_ThrownArgument()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = new WaveCatalog(Array.Empty<WaveDefinition>());
            });
        }

        [Test]
        public void Constructor_IfAnyWaveNull_ThrownArgument()
        {
            WaveNumber waveNumber = WaveNumber.First;

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new WaveCatalog(new[]
                {
                    new WaveDefinition(waveNumber, 1),
                    null,
                    new WaveDefinition(waveNumber.Next(), 3)
                });
            });
        }

        [Test]
        public void Constructor_IfNotSequence_ThrownArgument()
        {
            WaveNumber waveNumber = WaveNumber.First;

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new WaveCatalog(new[]
                {
                    new WaveDefinition(WaveNumber.First, 1),
                    new WaveDefinition(WaveNumber.FromValue(3), 1),
                    new WaveDefinition(WaveNumber.FromValue(2), 1),
                });
            });
        }

        [Test]
        public void ReturnWave_WhenGet_Correct()
        {
            int wavesCount = 3;
            List<WaveDefinition> waves = Enumerable
                .Range(1, wavesCount)
                .Select(n => new WaveDefinition(WaveNumber.FromValue(n), n))
                .ToList();
            WaveCatalog catalog = new(waves);

            for (int i = 0; i < wavesCount; i++)
            {
                WaveDefinition definition = catalog.Get(WaveNumber.FromValue(i + 1));

                Assert.That(definition.RegularEnemyCount == i + 1, Is.True);
                Assert.That(definition.Number.Value == i + 1, Is.True);
            }
        }

        [Test]
        public void Get_IfOutWaveNumber_Thrown()
        {
            WaveCatalog catalog = new(new[]
            {
                new WaveDefinition(WaveNumber.First, 1),
                new WaveDefinition(WaveNumber.FromValue(2), 2),
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                catalog.Get(WaveNumber.FromValue(3));
            });
        }

        [Test]
        public void IsLast_IfLast_ReturnTrue()
        {
            WaveCatalog catalog = new(new[]
{
                new WaveDefinition(WaveNumber.First, 1),
                new WaveDefinition(WaveNumber.FromValue(2), 2),
            });

            Assert.IsTrue(catalog.IsLast(WaveNumber.FromValue(2)));
        }

        [Test]
        public void IsLast_IfNotLast_ReturnFalse()
        {
            WaveCatalog catalog = new(new[]
{
                new WaveDefinition(WaveNumber.First, 1),
                new WaveDefinition(WaveNumber.FromValue(2), 2),
            });

            Assert.IsFalse(catalog.IsLast(WaveNumber.FromValue(1)));
        }

        [Test]
        public void LastProperty_ReturnLast()
        {
            WaveCatalog catalog = new(new[]
{
                new WaveDefinition(WaveNumber.First, 1),
                new WaveDefinition(WaveNumber.FromValue(2), 2),
            });

            Assert.That(catalog.Last.Value == 2, Is.True);
        }
    }
}

using System;
using System.Reflection;
using NUnit.Framework;

namespace Game.Architecture.Tests
{
    [TestFixture]
    public sealed class SnapshotArchitectureTests
    {
        [Test]
        public void ApplicationSnapshotsDoNotExposeArenaRun()
        {
            Type[] snapshotTypes =
            {
                "Game.Arena.Application.ReadModels.ArenaRunSnapshot".ResolveType("Game.Arena.Application"),
                "Game.Arena.Application.ReadModels.PlayerSnapshot".ResolveType("Game.Arena.Application"),
                "Game.Arena.Application.ReadModels.EnemySnapshot".ResolveType("Game.Arena.Application"),
                "Game.Arena.Application.ReadModels.WaveSnapshot".ResolveType("Game.Arena.Application"),
            };

            for (int typeIndex = 0; typeIndex < snapshotTypes.Length; typeIndex++)
            {
                Type snapshotType = snapshotTypes[typeIndex];

                PropertyInfo[] properties =
                    snapshotType.GetProperties(
                        BindingFlags.Instance
                        | BindingFlags.Public);

                for (int propertyIndex = 0;
                    propertyIndex < properties.Length;
                    propertyIndex++)
                {
                    PropertyInfo property = properties[propertyIndex];

                    Assert.That(
                        property.PropertyType.FullName,
                        Is.Not.EqualTo("Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun"),
                        snapshotType.FullName + "." + property.Name);
                }
            }
        }
    }
}

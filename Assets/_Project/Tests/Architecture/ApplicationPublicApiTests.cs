using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Assembly = System.Reflection.Assembly;

namespace Game.Architecture.Tests
{
    [TestFixture]
    public sealed class ApplicationPublicApiTests
    {
        private const string ArenaRunFullName = "Game.Arena.Domain.Aggregates.ArenaRun.ArenaRun";

        [Test]
        public void TickServiceDoesNotExposeArenaRun()
        {
            Type serviceType = LoadApplicationType("Game.Arena.Application.Ticks.ArenaRunTickService");

            foreach (MethodInfo method in serviceType.GetMethods(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            {
                if (method.IsSpecialName)
                {
                    continue;
                }

                Assert.That(
                    method.ReturnType.FullName,
                    Is.Not.EqualTo(ArenaRunFullName),
                    method.Name);

                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    Assert.That(
                        parameter.ParameterType.FullName,
                        Is.Not.EqualTo(ArenaRunFullName),
                        method.Name + "." + parameter.Name);
                }
            }

            foreach (PropertyInfo property in serviceType.GetProperties(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public))
            {
                Assert.That(
                    property.PropertyType.FullName,
                    Is.Not.EqualTo(ArenaRunFullName),
                    property.Name);
            }
        }

        [Test]
        public void TickInternalsRemainInternal()
        {
            string[] internalTypes =
            {
                "Game.Arena.Application.Ticks.ArenaRunTickCoordinator",
                "Game.Arena.Application.Ticks.ArenaRunTickCoordinatorDependencies",
                "Game.Arena.Application.Ticks.ArenaRunTickRecorder",
                "Game.Arena.Application.Ticks.PendingInteractionTracker",
                "Game.Arena.Application.Ticks.PlayerTickPhase",
                "Game.Arena.Application.Ticks.EnemyTickPhase",
                "Game.Arena.Application.Ticks.Stages.WeaponSwitchStage",
                "Game.Arena.Application.Ticks.Stages.PlayerMovementStage",
                "Game.Arena.Application.Ticks.Stages.PlayerAttackStartStage",
                "Game.Arena.Application.Ticks.Stages.PlayerAttackImpactStage",
                "Game.Arena.Application.Ticks.Stages.EnemyMovementStage",
                "Game.Arena.Application.Ticks.Stages.EnemyAttackStartStage",
                "Game.Arena.Application.Ticks.Stages.EnemyAttackImpactStage",
                "Game.Arena.Application.Ticks.Stages.EnemySpawnStage",
                "Game.Arena.Application.Ticks.Stages.PendingInteractionRecoveryStage",
                "Game.Arena.Application.Ticks.Stages.TimeAdvanceStage",
            };

            foreach (string fullName in internalTypes)
            {
                Type type = LoadApplicationType(fullName);

                Assert.That(type.IsPublic, Is.False, fullName);
                Assert.That(type.IsNotPublic, Is.True, fullName);
            }
        }

        private Type LoadApplicationType(string fullTypeName)
        {
            Assembly applicationAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Game.Arena.Application");

            Assert.That(
                applicationAssembly,
                Is.Not.Null,
                "Game.Arena.Application assembly was not found.");

            Type type = applicationAssembly.GetType(fullTypeName);

            Assert.That(type, Is.Not.Null, fullTypeName);

            return type;
        }
    }
}

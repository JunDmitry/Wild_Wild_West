using System;
using NUnit.Framework;
using UnityEditor.Compilation;

namespace Game.Architecture.Tests
{
    [TestFixture]
    public sealed class LegacyIsolationTests
    {
        private static readonly string[] s_legacyAssemblyNames =
        {
            "Game.Core.Model",
            "Game.Core.Rules",
            "Game.Application",
        };

        [Test]
        public void LegacyAssembliesDoNotReferenceArenaAssemblies()
        {
            Assembly[] assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);

            foreach (Assembly assembly in assemblies)
            {
                if (!IsLegacy(assembly.name))
                {
                    continue;
                }

                foreach (Assembly reference in assembly.assemblyReferences)
                {
                    bool referencesArena = reference.name.StartsWith("Game.Arena.", StringComparison.Ordinal)
                        || reference.name.Equals("Game.CompositionRoot", StringComparison.Ordinal);

                    Assert.That(referencesArena, Is.False, $"{assembly.name} references {reference.name}.");
                }
            }
        }

        private bool IsLegacy(string assemblyName)
        {
            foreach (string legacyName in s_legacyAssemblyNames)
            {
                if (assemblyName.Equals(legacyName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

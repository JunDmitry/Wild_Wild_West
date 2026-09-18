using System;
using System.IO;
using NUnit.Framework;
using UnityEditor.Compilation;
using UnityAssembly = UnityEditor.Compilation.Assembly;

namespace Game.Architecture.Tests
{
    [TestFixture]
    public sealed class IdentitySourceArchitectureTests
    {
        [Test]
        public void ArenaApplicationDoesNotReferenceLegacyIdentityAssembly()
        {
            UnityAssembly[] assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);
            UnityAssembly application = null;

            foreach (UnityAssembly assembly in assemblies)
            {
                if (assembly.name.Equals("Game.Arena.Application", StringComparison.Ordinal))
                {
                    application = assembly;
                    break;
                }
            }

            Assert.That(
                application,
                Is.Not.Null,
                "Game.Arena.Application assembly was not found.");

            foreach (string reference in application.compiledAssemblyReferences)
            {
                string referenceName = Path.GetFileNameWithoutExtension(reference);

                Assert.That(referenceName, Is.Not.EqualTo("Game.Application"));
                Assert.That(referenceName, Is.Not.EqualTo("Game.Core.Model"));
                Assert.That(referenceName, Is.Not.EqualTo("Game.Core.Rules"));
            }
        }
    }
}

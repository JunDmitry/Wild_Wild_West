using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;
using UnityAssembly = UnityEditor.Compilation.Assembly;

namespace Game.Architecture.Tests
{
    [TestFixture]
    public sealed class RepositoryContractTests
    {
        private Type _repositoryContractType;

        [SetUp]
        public void SetUp()
        {
            _repositoryContractType = ResolveDomainType("Game.Arena.Domain.Repositories.IArenaRunRepository");
        }

        [Test]
        public void ArenaRunRepositoryHasNoAsynchronousMembers()
        {
            MethodInfo[] methods = _repositoryContractType.GetMethods();

            foreach (MethodInfo method in methods)
            {
                bool returnsTask = typeof(System.Threading.Tasks.Task).IsAssignableFrom(method.ReturnType);

                Assert.That(
                    returnsTask,
                    Is.False,
                    $"{method.Name} must not be asynchronous at this stage.");
            }
        }

        [Test]
        public void ArenaRunRepositoryHasExactlyThreeMembers()
        {
            MethodInfo[] methods = _repositoryContractType.GetMethods();

            Assert.That(methods.Length, Is.EqualTo(3));
        }

        [Test]
        public void RepositoryContractDoesNotExposeOrmStyleMethods()
        {
            Type type = _repositoryContractType;

            Assert.That(type.GetMethod("Save"), Is.Null);
            Assert.That(type.GetMethod("Update"), Is.Null);
            Assert.That(type.GetMethod("SaveChanges"), Is.Null);
            Assert.That(type.GetMethod("GetAll"), Is.Null);
            Assert.That(type.GetMethod("FindBy"), Is.Null);
            Assert.That(type.GetMethod("Query"), Is.Null);
        }

        private Type ResolveDomainType(string fullTypeName)
        {
            UnityAssembly[] assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);
            UnityAssembly domainAssembly = assemblies.FirstOrDefault(assembly => assembly.name == "Game.Arena.Domain");

            Assert.That(
                domainAssembly,
                Is.Not.Null,
                "Game.Arena.Domain assembly was not found.");

            string assemblyPath = domainAssembly.outputPath;
            Assembly loadedAssembly = Assembly.LoadFrom(assemblyPath);
            Type type = loadedAssembly.GetType(fullTypeName);

            Assert.That(
                type,
                Is.Not.Null,
                $"Type was not found: {fullTypeName}.");

            return type;
        }
    }
}

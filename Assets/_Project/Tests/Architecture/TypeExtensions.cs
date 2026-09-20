using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;
using UnityAssembly = UnityEditor.Compilation.Assembly;

namespace Game.Architecture.Tests
{
    public static class TypeExtensions
    {
        public static Type ResolveType(this string fullTypeName, string assemblyName = null)
        {
            assemblyName ??= "Game.Arena.Domain";
            UnityAssembly[] assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);
            UnityAssembly domainAssembly = assemblies.FirstOrDefault(assembly => assembly.name == assemblyName);

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

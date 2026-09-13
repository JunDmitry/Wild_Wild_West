using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Game.Architecture.Tests
{
    [TestFixture]
    public sealed class AssemblyBoundaryTests
    {
        private const string GuidReferencePrefix = "GUID:";

        private readonly Dictionary<string, string[]> _allowedReferences = new(StringComparer.Ordinal)
        {
            ["Game.Arena.Domain"] = Array.Empty<string>(),
            ["Game.Arena.Application"] = new[]
                {
                    "Game.Arena.Domain",
                },
            ["Game.Arena.Infrastructure"] = new[]
                {
                    "Game.Arena.Domain",
                    "Game.Arena.Application",
                },
            ["Game.Arena.Presentation"] = new[]
                {
                    "Game.Arena.Application",
                },
            ["Game.CompositionRoot"] = new[]
                {
                    "Game.Arena.Domain",
                    "Game.Arena.Application",
                    "Game.Arena.Infrastructure",
                    "Game.Arena.Presentation",
                },
        };

        [TestCase("Game.Arena.Domain", true)]
        [TestCase("Game.Arena.Application", true)]
        [TestCase("Game.Arena.Infrastructure", false)]
        [TestCase("Game.Arena.Presentation", false)]
        [TestCase("Game.CompositionRoot", false)]
        public void AssemblyDefinitionUsesApprovedReferences(
            string assemblyName,
            bool noEngineReferences)
        {
            string path = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assemblyName);

            Assert.That(path, Is.Not.Null.And.Not.Empty,
                $"Assembly definition was not found: {assemblyName}.");

            AssemblyDefinitionData definition = ReadDefinition(path);
            List<string> referenceNames = new();

            foreach (string reference in definition.References)
            {
                referenceNames.Add(ResolveReferenceName(reference));
            }

            Assert.That(definition.Name, Is.EqualTo(assemblyName));
            Assert.That(definition.AutoReferenced, Is.False);
            Assert.That(definition.OverrideReferences, Is.True);
            Assert.That(definition.PrecompiledReferences, Is.Empty);
            Assert.That(definition.NoEngineReferences, Is.EqualTo(noEngineReferences));

            Assert.That(referenceNames, Is.EquivalentTo(_allowedReferences[assemblyName]),
                $"Unexpected dependencies of {assemblyName}.");
        }

        [TestCase("Game.Arena.Domain", true)]
        [TestCase("Game.Arena.Application", true)]
        [TestCase("Game.Arena.Infrastructure", false)]
        [TestCase("Game.Arena.Presentation", false)]
        [TestCase("Game.CompositionRoot", false)]
        public void PlayerCompilationGraphContainsNoForbiddenDependencies(
            string assemblyName,
            bool forbidUnity)
        {
            Assembly root = FindPlayerAssembly(assemblyName);
            Stack<Assembly> pending = new();
            HashSet<string> visited = new(StringComparer.Ordinal);

            pending.Push(root);

            while (pending.Count > 0)
            {
                Assembly current = pending.Pop();

                if (!visited.Add(current.name))
                {
                    continue;
                }

                AssertDependencyAllowed(
                    assemblyName,
                    current.name,
                    forbidUnity);

                foreach (string referencePath in current.compiledAssemblyReferences)
                {
                    string referenceName = Path.GetFileNameWithoutExtension(referencePath);

                    AssertDependencyAllowed(
                        assemblyName,
                        referenceName,
                        forbidUnity);
                }

                foreach (Assembly reference in current.assemblyReferences)
                {
                    pending.Push(reference);
                }
            }
        }

        [Test]
        public void ArenaAssemblyDefinitionsAreCoveredByDependencyContract()
        {
            string[] assetGuids = AssetDatabase.FindAssets(
                "t:AssemblyDefinitionAsset",
                new[] { "Assets" });

            foreach (string assetGuid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuid);
                AssemblyDefinitionData definition = ReadDefinition(path);

                bool isTargetAssembly = definition.Name.StartsWith("Game.Arena.", StringComparison.Ordinal)
                    || definition.Name.Equals("Game.CompositionRoot", StringComparison.Ordinal);

                if (!isTargetAssembly)
                {
                    continue;
                }

                Assert.That(_allowedReferences.ContainsKey(definition.Name), Is.True,
                    $"Dependency contract is missing for {definition.Name}.");
            }
        }

        private Assembly FindPlayerAssembly(string assemblyName)
        {
            Assembly[] assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.name.Equals(assemblyName, StringComparison.Ordinal))
                {
                    return assembly;
                }
            }

            throw new AssertionException($"Player assembly was not found: {assemblyName}.");
        }

        private void AssertDependencyAllowed(
            string rootAssemblyName,
            string dependencyName,
            bool forbidUnity)
        {
            bool isLegacy = dependencyName.StartsWith("Game.Core.", StringComparison.Ordinal)
                || dependencyName.Equals("Game.Application", StringComparison.Ordinal);

            Assert.That(
                isLegacy,
                Is.False,
                $"{rootAssemblyName} reaches legacy assembly {dependencyName}.");

            if (forbidUnity)
            {
                bool isUnity = dependencyName.StartsWith("UnityEngine", StringComparison.Ordinal)
                    || dependencyName.StartsWith("UnityEditor", StringComparison.Ordinal)
                    || dependencyName.StartsWith("Unity.", StringComparison.Ordinal);

                Assert.That(
                    isUnity,
                    Is.False,
                    $"{rootAssemblyName} reaches Unity assembly {dependencyName}.");
            }
        }

        private string ResolveReferenceName(string reference)
        {
            if (!reference.StartsWith(GuidReferencePrefix, StringComparison.Ordinal))
            {
                return reference;
            }

            string guid = reference[GuidReferencePrefix.Length..];
            string path = AssetDatabase.GUIDToAssetPath(guid);

            Assert.That(
                path,
                Is.Not.Null.And.Not.Empty,
                $"Assembly reference cannot be resolved: {reference}.");

            return this.ReadDefinition(path).Name;
        }

        private AssemblyDefinitionData ReadDefinition(string path)
        {
            string absolutePath = Path.GetFullPath(
                Path.Combine(
                    UnityEngine.Application.dataPath,
                    "..",
                    path));

            Assert.That(
                File.Exists(absolutePath),
                Is.True,
                $"Assembly definition does not exist: {path}.");

            string json = File.ReadAllText(absolutePath);
            AssemblyDefinitionData definition = JsonUtility.FromJson<AssemblyDefinitionData>(json);

            Assert.That(
                definition,
                Is.Not.Null,
                $"Assembly definition cannot be read: {path}.");

            return definition;
        }

#pragma warning disable IDE1006
        [Serializable]
        private sealed class AssemblyDefinitionData
        {
            [SerializeField]
            private string name = string.Empty;

            [SerializeField]
            private string[] references = Array.Empty<string>();

            [SerializeField]
            private bool autoReferenced = false;

            [SerializeField]
            private bool overrideReferences = false;

            [SerializeField]
            private string[] precompiledReferences = Array.Empty<string>();

            [SerializeField]
            private bool noEngineReferences = false;

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public string[] References
            {
                get
                {
                    return references ?? Array.Empty<string>();
                }
            }

            public bool AutoReferenced
            {
                get
                {
                    return autoReferenced;
                }
            }

            public bool OverrideReferences
            {
                get
                {
                    return overrideReferences;
                }
            }

            public string[] PrecompiledReferences
            {
                get
                {
                    return precompiledReferences ?? Array.Empty<string>();
                }
            }

            public bool NoEngineReferences
            {
                get
                {
                    return noEngineReferences;
                }
            }
        }
#pragma warning restore IDE1006
    }
}

using Assets.Scripts.Gameplay.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Architecture.CodeGeneration
{
#if UNITY_EDITOR

    public class ModelCodeGenerator
    {
        private static readonly CodeGenerator s_codeGenerator = new();
        private static readonly string s_projectDataPath = Application.dataPath;
        private static int s_lastModelsHash;

        [MenuItem("Tools/Generate model enum")]
        public static void GenerateModelHelpers()
        {
            string typeName = "ModelType";
            string mapperTypeName = "ModelMapper";

            Type[] modelTypes = GetModelTypes().ToArray();
            int currentHash = CalculateModelsHash(modelTypes);

            if (currentHash == s_lastModelsHash)
            {
                Debug.Log("Model types doesn't added or edited. Code not require generate!");
                return;
            }

            s_lastModelsHash = currentHash;

            GenerateModelEnums(modelTypes, typeName);
            GenerateModelByEnumMapper(modelTypes, mapperTypeName);
        }

        private static IEnumerable<Type> GetModelTypes()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type modelType = typeof(IModel);

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;

                    if (modelType.IsAssignableFrom(type) == false)
                        continue;

                    yield return type;
                }
            }
        }

        private static int CalculateModelsHash(Type[] models)
        {
            int currentHash = 1823;
            int multiplier = 31;

            foreach (Type type in models)
            {
                currentHash = currentHash * multiplier + GetModelName(type).GetHashCode();
            }

            return currentHash;
        }

        private static void GenerateModelEnums(Type[] modelTypes, string enumName)
        {
            string filePath = GetFilePath(enumName);
            int enumNum = 0;

            s_codeGenerator.AppendLine(string.Empty, isEndCodeLine: false)
                .AppendLine($"public enum {enumName}", isEndCodeLine: false)
                .PlaceOpenScope();

            foreach (Type modelType in modelTypes)
            {
                s_codeGenerator.AppendLine($"{GetModelName(modelType)} = {enumNum}, ", isEndCodeLine: false);

                enumNum++;
            }

            s_codeGenerator.PlaceCloseScope();

            WriteToFile(s_projectDataPath + filePath, s_codeGenerator.Build());
            s_codeGenerator.Clear();
        }

        private static void GenerateModelByEnumMapper(Type[] modelTypes, string mapperTypeName)
        {
            string filePath = GetFilePath(mapperTypeName);

            string constraintType = typeof(IModel).FullName;
            string keyType = typeof(Type).FullName;
            string valueType = typeof(ModelType).FullName;

            string modelTypeByModelFieldName = "s_modelTypeByModel";
            string modelByModelTypeFieldName = "s_modelByModelType";
            string modelTypeByModel = $"System.Collections.Generic.Dictionary<{keyType}, {valueType}>";
            string modelByModelType = $"System.Collections.Generic.Dictionary<{valueType}, {keyType}>";

            string methodName = "AsModelEnumType";
            string constraintName = "TModel";
            string variableName = "type";

            string enumMapMehtodName = "AsModelType";

            s_codeGenerator.AppendLine(string.Empty, isEndCodeLine: false)
                .AppendLine($"public static class {mapperTypeName}", isEndCodeLine: false)
                .PlaceOpenScope()
                .AppendLine($"private static readonly {modelTypeByModel} {modelTypeByModelFieldName}")
                .AppendLine($"private static readonly {modelByModelType} {modelByModelTypeFieldName}")
                .AppendLine(string.Empty, isEndCodeLine: false)
                .AppendLine($"static {mapperTypeName}()", isEndCodeLine: false)
                .PlaceOpenScope()
                .AppendLine($"{modelTypeByModelFieldName} = new {modelTypeByModel}({modelTypes.Length})")
                .AppendLine($"{modelByModelTypeFieldName} = new {modelByModelType}({modelTypes.Length})");

            foreach (Type modelType in modelTypes)
            {
                string enumModelName = GetModelName(modelType);
                s_codeGenerator.AppendLine(string.Empty, isEndCodeLine: false);
                s_codeGenerator.AppendLine($"{modelTypeByModelFieldName}[typeof({modelType.FullName})] = {valueType}.{enumModelName}");
                s_codeGenerator.AppendLine($"{modelByModelTypeFieldName}[{valueType}.{enumModelName}] = typeof({modelType.FullName})");
            }

            s_codeGenerator
                .PlaceCloseScope()
                .AppendLine(string.Empty, isEndCodeLine: false)
                .AppendLine($"public static {valueType} {methodName}<{constraintName}>()", isEndCodeLine: false)
                .AppendLine($"\twhere {constraintName} : {constraintType}", isEndCodeLine: false)
                .PlaceOpenScope()
                .AppendLine($"{keyType} {variableName} = typeof({constraintName})")
                .AppendLine($"return {modelTypeByModelFieldName}[{variableName}]")
                .PlaceCloseScope()
                .AppendLine(string.Empty, isEndCodeLine: false)
                .AppendLine($"public static {valueType} {methodName}({keyType} {variableName})", isEndCodeLine: false)
                .PlaceOpenScope()
                .AppendLine($"return {modelTypeByModelFieldName}[{variableName}]")
                .PlaceCloseScope()
                .AppendLine(string.Empty, isEndCodeLine: false)
                .AppendLine($"public static {keyType} {enumMapMehtodName}(this {valueType} {variableName})", isEndCodeLine: false)
                .PlaceOpenScope()
                .AppendLine($"return {modelByModelTypeFieldName}[{variableName}]")
                .PlaceCloseScope()
                .PlaceCloseScope();

            WriteToFile(s_projectDataPath + filePath, s_codeGenerator.Build());
            s_codeGenerator.Clear();
        }

        private static string GetModelName(Type type)
        {
            string name;

            if (type.IsGenericType)
            {
                name = type.Name[..type.Name.IndexOf('`')];
                string args = string.Join("And", type.GetGenericArguments().Select(GetModelName));
                name = $"{name}Of{args}";
            }
            else
            {
                int index = type.FullName.LastIndexOf('.');
                name = index < 0 ? type.FullName : type.FullName[(index + 1)..];
            }

            return name;
        }

        private static void WriteToFile(string filePath, string content)
        {
            string directory = Path.GetDirectoryName(filePath);

            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, content);
            Debug.Log($"Generated: {filePath}");
        }

        private static string GetFilePath(string fileName)
        {
            return $"/Scripts/Generated/{fileName}.cs";
        }
    }

#endif
}
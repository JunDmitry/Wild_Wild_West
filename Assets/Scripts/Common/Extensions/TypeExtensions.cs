using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Common.Extensions
{
    /// <summary>
    /// Provides convenient methods for scanning the <see cref="System.Type">types</see> of the Current Domain <br/>
    /// Has heavy lazy initializable static cache about <see cref="System.Type">types</see> in CurrentDomain
    /// </summary>
    public static class TypeExtensions
    {
        private static Type[] s_cachedDomainTypes = null;

        /// <summary>
        /// Find all non-abstract class types assignable from type with option generic search flag. 
        /// Analysis all assemblies in CurrentDomain
        /// </summary>
        /// <param name="type">Assignable from</param>
        /// <param name="isGenericType">Search generic or non-generic types</param>
        /// <exception cref="System.ArgumentNullException">If <paramref name="type"/> is null</exception>
        /// <returns>All non-abstract class types assignable from <paramref name="type"/></returns>
        public static IEnumerable<Type> FindAllNonAbstractClassAssignableFrom(this Type type, bool isGenericType = false)
        {
            return FindAllAssignableFrom(type,
                t => (t.IsAbstract == false) && t.IsClass && (t.IsGenericType == isGenericType));
        }

        /// <summary>
        /// Find all non-abstract class types assignable from type with all possible search configure option Type.Is* flags. 
        /// Analysis all assemblies in CurrentDomain
        /// </summary>
        /// <param name="type">Assignable from</param>
        /// <param name="condition">Type enum flags from <see cref="System.Type">Type.Is*</see></param>
        /// <exception cref="System.ArgumentNullException">If <paramref name="type"/> is null</exception>
        /// <returns>All types assignable from <paramref name="type"/> and satisfied <paramref name="condition"/> flag</returns>
        public static IEnumerable<Type> FindAllAssignableFrom(this Type type, TypeCondition condition)
        {
            if (condition == TypeCondition.None)
                return FindAllAssignableFrom(type);

            return FindAllAssignableFrom(type, t => (GetFlag(t) & condition) == condition);
        }

        /// <summary>
        /// Find all types assignable from <paramref name="type"/>. Analysis all assemblies in CurrentDomain
        /// </summary>
        /// <param name="type">Assignable from</param>
        /// <param name="additionAndPredicates">Additional predicates that applies to founded candidate type</param>
        /// <exception cref="System.ArgumentNullException">If <paramref name="type"/> is null</exception>
        /// <returns>All assignable from <paramref name="type"/> types</returns>
        public static IEnumerable<Type> FindAllAssignableFrom(this Type type, params Func<Type, bool>[] additionAndPredicates)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (s_cachedDomainTypes == null)
                LoadAllDomainTypes();

            additionAndPredicates ??= Array.Empty<Func<Type, bool>>();

            return FindAllAssignableFromInternal(type, additionAndPredicates);
        }

        /// <summary>
        /// Clear static cache about types in CurrentDomain
        /// </summary>
        public static void ClearCache()
        {
            s_cachedDomainTypes = null;
        }

        private static void LoadAllDomainTypes()
        {
            s_cachedDomainTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x =>
            {
                try
                {
                    return x.GetTypes();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                    return Enumerable.Empty<Type>();
                }
            }).ToArray();
        }

        private static IEnumerable<Type> FindAllAssignableFromInternal(Type type, Func<Type, bool>[] additionAndPredicates)
        {
            foreach (Type candidateType in s_cachedDomainTypes)
            {
                if (type.IsAssignableFrom(candidateType) == false)
                    continue;

                bool isAssignable = additionAndPredicates.All(p => p == null || p(candidateType));

                if (isAssignable)
                    yield return candidateType;
            }
        }

        private static TypeCondition GetFlag(Type type)
        {
            TypeCondition condition = TypeCondition.None;

            if (type.IsAbstract)
                condition |= TypeCondition.Abstract;

            if (type.IsAnsiClass)
                condition |= TypeCondition.AnsiClass;

            if (type.IsArray)
                condition |= TypeCondition.Array;

            if (type.IsAutoClass)
                condition |= TypeCondition.AutoClass;

            if (type.IsAutoLayout)
                condition |= TypeCondition.AutoLayout;

            if (type.IsByRef)
                condition |= TypeCondition.ByRef;

            if (type.IsByRefLike)
                condition |= TypeCondition.ByRefLike;

            if (type.IsClass)
                condition |= TypeCondition.Class;

            if (type.IsCOMObject)
                condition |= TypeCondition.COMObject;

            if (type.IsConstructedGenericType)
                condition |= TypeCondition.ConstructedGenericType;

            if (type.IsContextful)
                condition |= TypeCondition.Contextful;

            if (type.IsEnum)
                condition |= TypeCondition.Enum;

            if (type.IsExplicitLayout)
                condition |= TypeCondition.ExplicitLayout;

            if (type.IsGenericMethodParameter)
                condition |= TypeCondition.GenericMethodParameter;

            if (type.IsGenericParameter)
                condition |= TypeCondition.GenericParameter;

            if (type.IsGenericType)
                condition |= TypeCondition.GenericType;

            if (type.IsGenericTypeDefinition)
                condition |= TypeCondition.GenericTypeDefinition;

            if (type.IsGenericTypeParameter)
                condition |= TypeCondition.GenericTypeParameter;

            if (type.IsImport)
                condition |= TypeCondition.Import;

            if (type.IsInterface)
                condition |= TypeCondition.Interface;

            if (type.IsLayoutSequential)
                condition |= TypeCondition.LayoutSequential;

            if (type.IsMarshalByRef)
                condition |= TypeCondition.MarshalByRef;

            if (type.IsNested)
                condition |= TypeCondition.Nested;

            if (type.IsNestedAssembly)
                condition |= TypeCondition.NestedAssembly;

            if (type.IsNestedFamANDAssem)
                condition |= TypeCondition.NestedFamAndAssem;

            if (type.IsNestedFamily)
                condition |= TypeCondition.NestedFamily;

            if (type.IsNestedFamORAssem)
                condition |= TypeCondition.NestedFamOrAssem;

            if (type.IsNestedPublic)
                condition |= TypeCondition.NestedPublic;

            if (type.IsNestedPrivate)
                condition |= TypeCondition.NestedPrivate;

            if (type.IsNotPublic)
                condition |= TypeCondition.NotPublic;

            if (type.IsPointer)
                condition |= TypeCondition.Pointer;

            if (type.IsPrimitive)
                condition |= TypeCondition.Primitive;

            if (type.IsPublic)
                condition |= TypeCondition.Public;

            if (type.IsSealed)
                condition |= TypeCondition.Sealed;

            if (type.IsSecurityCritical)
                condition |= TypeCondition.SecurityCritical;

            if (type.IsSecuritySafeCritical)
                condition |= TypeCondition.SecuritySafeCritical;

            if (type.IsSecurityTransparent)
                condition |= TypeCondition.SecurityTransparent;

            if (type.IsSerializable)
                condition |= TypeCondition.Serializable;

            if (type.IsSignatureType)
                condition |= TypeCondition.SignatureType;

            if (type.IsSpecialName)
                condition |= TypeCondition.SpecialName;

            if (type.IsSZArray)
                condition |= TypeCondition.SZArray;

            if (type.IsTypeDefinition)
                condition |= TypeCondition.TypeDefinition;

            if (type.IsUnicodeClass)
                condition |= TypeCondition.UnicodeClass;

            if (type.IsValueType)
                condition |= TypeCondition.ValueType;

            if (type.IsVariableBoundArray)
                condition |= TypeCondition.VariableBoundArray;

            if (type.IsVisible)
                condition |= TypeCondition.Visible;
            
            return condition;
        }

        /// <summary>
        /// Contains all the flag properties defined in an instance of type <see cref="System.Type">System.Type</see>
        /// </summary>
        [Flags]
        public enum TypeCondition : long
        {
            None = 0,

            /// <summary>
            /// Value indicating whether the Type is abstract and must be overridden
            /// </summary>
            Abstract = 1L << 0,

            /// <summary>
            /// Value indicating whether the string format attribute AnsiClass is selected for the Type
            /// </summary>
            AnsiClass = 1L << 1,

            /// <summary>
            /// Value that indicates whether the type is an array
            /// </summary>
            Array = 1L << 2,

            /// <summary>
            /// Value indicating whether the string format attribute AutoClass is selected for the Type.
            /// </summary>
            AutoClass = 1L << 3,

            /// <summary>
            /// Value indicating whether the fields of the current type are laid out automatically by the common language runtime.
            /// </summary>
            AutoLayout = 1L << 4,

            /// <summary>
            /// Value indicating whether the Type is passed by reference.
            /// </summary>
            ByRef = 1L << 5,

            /// <summary>
            /// Value that indicates whether the type is a byref-like structure.
            /// </summary>
            ByRefLike = 1L << 6,

            /// <summary>
            /// Value indicating whether the Type is a class or a delegate; that is, not a value type or interface.
            /// </summary>
            Class = 1L << 7,

            /// <summary>
            /// Value indicating whether the Type is a COM object.
            /// </summary>
            COMObject = 1L << 8,

            /// <summary>
            /// Value indicating whether the Type can be hosted in a context.
            /// </summary>
            Contextful = 1L << 9,

            /// <summary>
            /// Value indicating whether the current Type represents an enumeration.
            /// </summary>
            Enum = 1L << 10,

            /// <summary>
            /// Value indicating whether the fields of the current type are laid out at explicitly specified offsets.
            /// </summary>
            ExplicitLayout = 1L << 11,

            /// <summary>
            /// Value that indicates whether the current Type represents a type parameter in the definition of a generic method.
            /// </summary>
            GenericMethodParameter = 1L << 12,

            /// <summary>
            /// Value indicating whether the current Type represents a type parameter in the definition of a generic type or method.
            /// </summary>
            GenericParameter = 1L << 13,

            /// <summary>
            /// Value indicating whether the current type is a generic type.
            /// </summary>
            GenericType = 1L << 14,

            /// <summary>
            /// Value indicating whether the current Type represents a generic type definition, from which other generic types can be constructed.
            /// </summary>
            GenericTypeDefinition = 1L << 15,

            /// <summary>
            /// Value that indicates whether the current Type represents a type parameter in the definition of a generic type.
            /// </summary>
            GenericTypeParameter = 1L << 16,

            /// <summary>
            /// Value indicating whether the Type has a ComImportAttribute attribute applied, indicating that it was imported from a COM type library.
            /// </summary>
            Import = 1L << 17,

            /// <summary>
            /// Value indicating whether the Type is an interface; that is, not a class or a value type.
            /// </summary>
            Interface = 1L << 18,

            /// <summary>
            /// Value indicating whether the fields of the current type are laid out sequentially, in the order that they were defined or emitted to the metadata.
            /// </summary>
            LayoutSequential = 1L << 19,

            /// <summary>
            /// Value indicating whether the Type is marshaled by reference.
            /// </summary>
            MarshalByRef = 1L << 20,

            /// <summary>
            /// Value indicating whether the current Type object represents a type whose definition is nested inside the definition of another type.
            /// </summary>
            Nested = 1L << 21,

            /// <summary>
            /// Value indicating whether the Type is nested and visible only within its own assembly.
            /// </summary>
            NestedAssembly = 1L << 22,

            /// <summary>
            /// Value indicating whether the Type is nested and visible only to classes that belong to both its own family and its own assembly.
            /// </summary>
            NestedFamAndAssem = 1L << 23,

            /// <summary>
            /// Value indicating whether the Type is nested and visible only within its own family.
            /// </summary>
            NestedFamily = 1L << 24,

            /// <summary>
            /// Value indicating whether the Type is nested and visible only to classes that belong to either its own family or to its own assembly.
            /// </summary>
            NestedFamOrAssem = 1L << 25,

            /// <summary>
            /// Value indicating whether a class is nested and declared public.
            /// </summary>
            NestedPublic = 1L << 26,

            /// <summary>
            /// Value indicating whether the Type is nested and declared private.
            /// </summary>
            NestedPrivate = 1L << 27,

            /// <summary>
            /// Value indicating whether the Type is not declared public.
            /// </summary>
            NotPublic = 1L << 28,

            /// <summary>
            /// Value indicating whether the Type is a pointer.
            /// </summary>
            Pointer = 1L << 29,

            /// <summary>
            /// Value indicating whether the Type is one of the primitive types.
            /// </summary>
            Primitive = 1L << 30,

            /// <summary>
            /// Value indicating whether the Type is declared public.
            /// </summary>
            Public = 1L << 31,

            /// <summary>
            /// Value indicating whether the Type is declared sealed.
            /// </summary>
            Sealed = 1L << 32,

            /// <summary>
            /// Value that indicates whether the current type is security-critical or security-safe-critical <br/>
            /// at the current trust level, and therefore can perform critical operations.
            /// </summary>
            SecurityCritical = 1L << 33,

            /// <summary>
            /// Value that indicates whether the current type is security-safe-critical at the current trust level; <br/>
            /// that is, whether it can perform critical operations and can be accessed by transparent code.
            /// </summary>
            SecuritySafeCritical = 1L << 34,

            /// <summary>
            /// Value that indicates whether the current type is transparent at the current trust level, and therefore cannot perform critical operations.
            /// </summary>
            SecurityTransparent = 1L << 35,

            /// <summary>
            /// Value indicating whether the Type is binary serializable.
            /// </summary>
            [Obsolete("Formatter-based serialization is obsolete and should not be used.")]
            Serializable = 1L << 36,

            /// <summary>
            /// Value that indicates whether the type is a signature type.
            /// </summary>
            SignatureType = 1L << 37,

            /// <summary>
            /// Value indicating whether the type has a name that requires special handling.
            /// </summary>
            SpecialName = 1L << 38,

            /// <summary>
            /// Value that indicates whether the type is an array type that can represent only a single-dimensional array with a zero lower bound.
            /// </summary>
            SZArray = 1L << 39,

            /// <summary>
            /// Value that indicates whether the type is a type definition.
            /// </summary>
            TypeDefinition = 1L << 40,

            /// <summary>
            /// Value indicating whether the string format attribute UnicodeClass is selected for the Type.
            /// </summary>
            UnicodeClass = 1L << 41,

            /// <summary>
            /// Value indicating whether the Type is a value type.
            /// </summary>
            ValueType = 1L << 42,

            /// <summary>
            /// Value that indicates whether the type is an array type that can represent a multi-dimensional array or an array with an arbitrary lower bound.
            /// </summary>
            VariableBoundArray = 1L << 43,

            /// <summary>
            /// Value indicating whether the Type can be accessed by code outside the assembly.
            /// </summary>
            Visible = 1L << 44,

            /// <summary>
            /// Value that indicates whether this object represents a constructed generic type.
            /// </summary>
            ConstructedGenericType = 1L << 45,
        }
    }
}

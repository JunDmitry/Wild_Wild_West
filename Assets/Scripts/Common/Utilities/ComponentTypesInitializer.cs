using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Gameplay.Common.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Common.Utilities
{
    public static class ComponentTypesInitializer
    {
        [RuntimeInitializeOnLoadMethod]
        private static void InitializeIdentifiers()
        {
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes()
                    .Where(t => (t.IsAbstract == false) && (t.IsInterface == false) && typeof(ComponentData).IsAssignableFrom(t)));

            foreach (Type type in types)
                type.GetId();
        }
    }
}

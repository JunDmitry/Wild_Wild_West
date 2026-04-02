#if UNITY_EDITOR

using System;

#endif

using UnityEngine;

namespace Assets.Scripts.Utility
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class SubClass : PropertyAttribute
    {
        public SubClass()
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{nameof(SubClass)} {nameof(PropertyAttribute)} should be used with [{nameof(SerializeReference)}]");
#endif
        }
    }

#if UNITY_EDITOR

#endif
}
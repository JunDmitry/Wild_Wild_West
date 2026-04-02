#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

#endif

using UnityEngine;

namespace Assets.Scripts.Utility
{
#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(SubClass))]
    public class SubClassDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, List<Type>> s_subclassesByAbstractType = new();

        private readonly SubclassSearchProvider _searchProvider;

        public SubClassDrawer()
        {
            _searchProvider = ScriptableObject.CreateInstance<SubclassSearchProvider>();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };

            Button dropdownButton = new(() =>
            {
                Type fieldType = fieldInfo.FieldType;

                if (fieldType == null)
                    return;

                List<Type> subclasses = GetSubclasses(fieldType);
                _searchProvider.Initialize(subclasses, GetOnSelectedTypeAction(property));

                SearchWindow.Open(
                    new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                    _searchProvider);
            })
            {
                text = "🔽",
                style =
                {
                    width = 20,
                    height = 20,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginLeft = 5,
                }
            };
            PropertyField propertyField = new(property)
            {
                style =
                {
                    flexGrow = 1,
                    marginLeft = 10,
                }
            };

            container.Add(propertyField);
            container.Add(dropdownButton);

            return container;
        }

        private static List<Type> GetSubclasses(Type baseType)
        {
            if (s_subclassesByAbstractType.TryGetValue(baseType, out List<Type> subclasses) == false)
            {
                subclasses = Assembly.GetAssembly(baseType)
                    .GetTypes()
                    .Where(t => t.IsClass && (t.IsAbstract == false) && baseType.IsAssignableFrom(t))
                    .ToList();
                s_subclassesByAbstractType[baseType] = subclasses;
            }

            return subclasses;
        }

        private static Action<Type> GetOnSelectedTypeAction(SerializedProperty property)
        {
            return selectedType =>
            {
                property.serializedObject.Update();
                property.managedReferenceValue = Activator.CreateInstance(selectedType);
                property.serializedObject.ApplyModifiedProperties();
            };
        }

        private class SubclassSearchProvider : ScriptableObject, ISearchWindowProvider
        {
            private List<Type> _subclasses;
            private Action<Type> _onSelectedType;

            public void Initialize(List<Type> subclasses, Action<Type> onSelectedType)
            {
                _subclasses = subclasses;
                _onSelectedType = onSelectedType;
            }

            List<SearchTreeEntry> ISearchWindowProvider.CreateSearchTree(SearchWindowContext context)
            {
                List<SearchTreeEntry> tree = new()
                {
                    new SearchTreeGroupEntry(new("Select type"), 0)
                };

                foreach (Type subclass in _subclasses)
                {
                    tree.Add(new SearchTreeEntry(new(subclass.Name))
                    {
                        level = 1,
                        userData = subclass
                    });
                }

                return tree;
            }

            bool ISearchWindowProvider.OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
            {
                if (searchTreeEntry.userData is not Type selectedType)
                    return false;

                _onSelectedType?.Invoke(selectedType);

                return true;
            }
        }
    }

#endif
}
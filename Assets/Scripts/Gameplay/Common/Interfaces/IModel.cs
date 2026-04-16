using System;
using System.Collections.Generic;
using Assets.Scripts.Gameplay.Configs;

namespace Assets.Scripts.Gameplay.Common.Interfaces
{
    public interface IModel
    {
        int Id { get; }
    }

    public interface IModel<out TData> : IModel
        where TData : IModelData
    {
        TData Data { get; }
    }

    public interface IComponentModel : IModel
    {
        IReadOnlyDictionary<int, IReadOnlyComponentData> StaticData { get; }

        T Get<T>() where T : ComponentData;
        ComponentData Get(int id);
        void Add<T>(T componentData) where T : ComponentData;
        void Replace<T>(T componentData) where T : ComponentData;
        void Remove<T>() where T : ComponentData;
        bool Has<T>() where T : ComponentData;
        bool Has(Type type);
        bool Has(int id);
    }

    public interface IReadOnlyComponentData
    {
        int TypeId { get; }
    }

    [Serializable]
    public abstract class ComponentData : IReadOnlyComponentData
    {
        private int _typeId;

        public int TypeId
        {
            get
            {
                if (_typeId == 0)
                    _typeId = GetType().GetId();

                return _typeId;
            }
        }
        
        public abstract ComponentData CloneDeep();
    }

    public static class TypeIdentifier
    {
        private static Dictionary<Type, int> s_idByType = new();
        private static int s_id = 1;

        public static int GetId(this Type type)
        {
            if (type.IsAbstract || type.IsInterface)
                return default;

            if (s_idByType.TryGetValue(type, out int id) == false)
            {
                id = s_id++;
                s_idByType[type] = id;
            }
            
            return id;
        }
    }
}
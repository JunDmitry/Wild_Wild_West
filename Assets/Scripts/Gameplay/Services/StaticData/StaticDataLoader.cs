using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public class StaticDataLoader<TEnumType, TStaticType>
        where TEnumType : Enum
        where TStaticType : ScriptableObject
    {
        private readonly string _staticDataPath;
        private readonly Func<TStaticType, TEnumType> _keySelector;

        private Dictionary<TEnumType, TStaticType> _staticDataByEnum;
        private bool _isLoaded = false;

        public StaticDataLoader(string staticDataPath, Func<TStaticType, TEnumType> keySelector)
        {
            ThrowIf.Null(staticDataPath, nameof(staticDataPath));
            ThrowIf.Null(keySelector, nameof(keySelector));

            _staticDataPath = staticDataPath;
            _keySelector = keySelector;
            _staticDataByEnum = new();
        }

        public void Load()
        {
            if (_isLoaded)
                return;

            _isLoaded = true;

            foreach (TStaticType staticType in Resources.LoadAll<TStaticType>(_staticDataPath))
            {
                TEnumType enumType = _keySelector(staticType);

                ThrowIf.Invalid(_staticDataByEnum.ContainsKey(enumType),
                    $"For each {nameof(TEnumType)} can be create only 1 {nameof(TStaticType)}. Error in {typeof(TEnumType).Name} {typeof(TStaticType).Name}");

                _staticDataByEnum[enumType] = staticType;
            }
        }

        public TStaticType GetConfig(TEnumType enumType)
        {
            if (TryGetConfig(enumType, out TStaticType config) == false)
                throw new KeyNotFoundException($"{typeof(TEnumType).Name} data for {enumType} was not found");

            return config;
        }

        public bool TryGetConfig(TEnumType enumType, out TStaticType config)
        {
            return _staticDataByEnum.TryGetValue(enumType, out config);
        }
    }
}
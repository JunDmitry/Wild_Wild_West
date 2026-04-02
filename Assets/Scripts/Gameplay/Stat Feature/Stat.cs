using UnityEngine;

namespace Assets.Scripts.Gameplay.Stat_Feature
{
    public class Stat : IStat
    {
        public static readonly Stat Empty = new(new(StatType.None, 0, 0, 0));

        private readonly StatDescriptor _statDescriptor;

        private float _baseValue;
        private float _currentValue;

        public Stat(StatDescriptor statDescriptor)
        {
            _statDescriptor = statDescriptor;
            _baseValue = _statDescriptor.BaseValue;
            _currentValue = _baseValue;
        }

        public StatType Type => _statDescriptor.Type;
        public float BaseValue => _baseValue;
        public float CurrentValue => _currentValue;

        public float Normalized
        {
            get
            {
                if (Mathf.Approximately(_statDescriptor.MinValue, _statDescriptor.MaxValue))
                    return 0f;

                return (_currentValue + -_statDescriptor.MinValue) / (_statDescriptor.MaxValue - _statDescriptor.MinValue);
            }
        }

        public void ChangeBase(float value)
        {
            _baseValue = Mathf.Clamp(value, _statDescriptor.MinValue, _statDescriptor.MaxValue);
        }

        public void ChangeCurrent(float value)
        {
            _currentValue = Mathf.Clamp(value, _statDescriptor.MinValue, _statDescriptor.MaxValue);
        }

        public void Reset()
        {
            _baseValue = _statDescriptor.BaseValue;
            _currentValue = _baseValue;
        }
    }
}
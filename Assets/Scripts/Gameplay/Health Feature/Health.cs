using System;

namespace Assets.Scripts.Gameplay.HealthFeature
{
    public class Health : IHealth
    {
        private readonly HealthContext _healthContext;

        public Health(int id, HealthContext healthContext)
        {
            _healthContext = healthContext;
            Current = Max;
            Id = id;
        }

        public event Action<float> CurrentChanged;

        public int Id { get; }
        public float Current { get; private set; }
        public float Max => _healthContext.Max;

        public void TakeDamage(float damage)
        {
            ThrowIf.Invalid(damage < 0, $"{nameof(damage)} should be positive");

            Current = Math.Max(Current - damage, 0);
            CurrentChanged?.Invoke(Current);
        }

        public void Replenish(float count)
        {
            ThrowIf.Invalid(count < 0, "Replenish health count should be positive");

            Current = Math.Min(Current + count, _healthContext.Max);
            CurrentChanged?.Invoke(Current);
        }
    }
}
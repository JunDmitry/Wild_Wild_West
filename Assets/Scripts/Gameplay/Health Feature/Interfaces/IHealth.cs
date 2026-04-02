using Assets.Scripts.Gameplay.Common.Interfaces;

namespace Assets.Scripts.Gameplay.HealthFeature
{
    public interface IHealth : IModel
    {
        float Current { get; }
        float Max { get; }
        void TakeDamage(float damage);
        void Replenish(float count);
    }
}
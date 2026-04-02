namespace Assets.Scripts.Gameplay.Stat_Feature
{
    public interface IStat
    {
        StatType Type { get; }
        float BaseValue { get; }
        float CurrentValue { get; }
    }
}
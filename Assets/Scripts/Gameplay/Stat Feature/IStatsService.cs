namespace Assets.Scripts.Gameplay.Stat_Feature
{
    public interface IStatsService
    {
        bool TryGetStat(int id, StatType statType, out Stat stat);

        Stat GetStat(int id, StatType statType);

        bool HasStat(int id, StatType statType);

        bool HasStats(int id, params StatType[] statTypes);
    }
}
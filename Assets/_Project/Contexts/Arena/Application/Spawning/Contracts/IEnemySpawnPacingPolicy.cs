using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Spawning
{
    public interface IEnemySpawnPacingPolicy
    {
        bool ShouldRequestSpawn(GameTimePoint now);
        void RecordSuccessfulSpawn(GameTimePoint now);
        void Reset();
    }
}

using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Spawning
{
    public sealed class FixedIntervalSpawnPacingPolicy : IEnemySpawnPacingPolicy
    {
        private readonly GameDuration _interval;

        private bool _hasPreviousSpawn;
        private GameTimePoint _lastSpawnTime;

        public FixedIntervalSpawnPacingPolicy(GameDuration interval)
        {
            if (interval.Seconds <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(interval));
            }

            _interval = interval;
            _hasPreviousSpawn = false;
            _lastSpawnTime = new GameTimePoint(0d);
        }

        public void RecordSuccessfulSpawn(GameTimePoint now)
        {
            _hasPreviousSpawn = true;
            _lastSpawnTime = now;
        }

        public void Reset()
        {
            _hasPreviousSpawn = false;
            _lastSpawnTime = new GameTimePoint(0d);
        }

        public bool ShouldRequestSpawn(GameTimePoint now)
        {
            if (_hasPreviousSpawn == false)
            {
                return true;
            }

            return now >= _lastSpawnTime + _interval;
        }
    }
}

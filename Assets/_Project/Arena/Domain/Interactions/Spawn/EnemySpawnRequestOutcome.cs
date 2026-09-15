namespace Game.Arena.Domain.Interactions.Spawn
{
    public sealed class EnemySpawnRequestOutcome
    {
        private EnemySpawnRequestOutcome(EnemySpawnRequestStatus status, EnemySpawnRequest request)
        {
            Status = status;
            Request = request;
        }
        public static EnemySpawnRequestOutcome RunIsNotPlaying { get; } = new EnemySpawnRequestOutcome(EnemySpawnRequestStatus.RunIsNotPlaying, default);
        public static EnemySpawnRequestOutcome NoSpawnDue { get; } = new EnemySpawnRequestOutcome(EnemySpawnRequestStatus.NoSpawnDue, default);

        public EnemySpawnRequestStatus Status { get; }

        public EnemySpawnRequest Request { get; }

        public bool HasRequest => Status == EnemySpawnRequestStatus.Requested;

        public static EnemySpawnRequestOutcome Requested(EnemySpawnRequest request)
        {
            return new EnemySpawnRequestOutcome(EnemySpawnRequestStatus.Requested, request);
        }
    }
}

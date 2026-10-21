namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class EnemyMovementBatchRequestOutcome
    {
        private EnemyMovementBatchRequestOutcome(
            EnemyMovementBatchRequestStatus status,
            EnemyMovementBatchRequest request)
        {
            Status = status;
            Request = request;
        }

        public static EnemyMovementBatchRequestOutcome RunIsNotPlaying => new(EnemyMovementBatchRequestStatus.RunIsNotPlaying, null);

        public static EnemyMovementBatchRequestOutcome NoEligibleEnemies => new(EnemyMovementBatchRequestStatus.NoEligibleEnemies, null);

        public EnemyMovementBatchRequestStatus Status { get; }

        public EnemyMovementBatchRequest Request { get; }

        public bool HasRequest => Status == EnemyMovementBatchRequestStatus.Requested;

        public static EnemyMovementBatchRequestOutcome Requested(EnemyMovementBatchRequest request)
        {
            return new EnemyMovementBatchRequestOutcome(EnemyMovementBatchRequestStatus.Requested, request);
        }
    }
}

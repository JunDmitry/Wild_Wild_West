namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class EnemyMovementBatchRequestOutcome
    {
        internal EnemyMovementBatchRequestOutcome()
        {
        }

        private EnemyMovementBatchRequestOutcome(
            EnemyMovementBatchRequestStatus status,
            EnemyMovementBatchRequest request)
        {
            Status = status;
            Request = request;
        }

        public static EnemyMovementBatchRequestOutcome RunIsNotPlaying { get; } = new(EnemyMovementBatchRequestStatus.RunIsNotPlaying, null);

        public static EnemyMovementBatchRequestOutcome NoEligibleEnemies { get; } = new(EnemyMovementBatchRequestStatus.NoEligibleEnemies, null);

        public EnemyMovementBatchRequestStatus Status { get; private set; }

        public EnemyMovementBatchRequest Request { get; private set; }

        public bool HasRequest => Status == EnemyMovementBatchRequestStatus.Requested;

        internal void Fill(EnemyMovementBatchRequestStatus status, EnemyMovementBatchRequest request)
        {
            Status = status;
            Request = request;
        }

        public static EnemyMovementBatchRequestOutcome Requested(EnemyMovementBatchRequest request)
        {
            return new EnemyMovementBatchRequestOutcome(EnemyMovementBatchRequestStatus.Requested, request);
        }
    }
}

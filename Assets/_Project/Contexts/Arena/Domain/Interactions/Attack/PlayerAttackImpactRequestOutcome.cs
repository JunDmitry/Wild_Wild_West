namespace Game.Arena.Domain.Interactions.Attack
{
    public sealed class PlayerAttackImpactRequestOutcome
    {
        internal PlayerAttackImpactRequestOutcome()
        {
        }

        private PlayerAttackImpactRequestOutcome(
            PlayerAttackImpactRequestStatus status,
            PlayerAttackImpactRequest request)
        {
            Status = status;
            Request = request;
        }

        public static PlayerAttackImpactRequestOutcome RunIsNotPlaying { get; } = new(PlayerAttackImpactRequestStatus.RunIsNotPlaying, default);
        public static PlayerAttackImpactRequestOutcome NoPendingAttack { get; } = new(PlayerAttackImpactRequestStatus.NoPendingAttack, default);
        public static PlayerAttackImpactRequestOutcome ImpactNotDue { get; } = new(PlayerAttackImpactRequestStatus.ImpactNotDue, default);

        public PlayerAttackImpactRequestStatus Status { get; private set; }

        public PlayerAttackImpactRequest Request { get; private set; }

        public bool HasRequest => Status == PlayerAttackImpactRequestStatus.Requested;

        internal void Fill(PlayerAttackImpactRequestStatus status, PlayerAttackImpactRequest request)
        {
            Status = status;
            Request = request;
        }

        public static PlayerAttackImpactRequestOutcome Requested(PlayerAttackImpactRequest request)
        {
            return new PlayerAttackImpactRequestOutcome(PlayerAttackImpactRequestStatus.Requested, request);
        }
    }
}

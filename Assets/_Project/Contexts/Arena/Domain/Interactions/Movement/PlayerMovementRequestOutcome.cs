using System;

namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class PlayerMovementRequestOutcome
    {
        internal PlayerMovementRequestOutcome()
        {
        }

        private PlayerMovementRequestOutcome(
            PlayerMovementRequestStatus status,
            PlayerMovementRequest request)
        {
            Validate(status, request);
            Status = status;
            Request = request;
        }

        public static PlayerMovementRequestOutcome NotPlaying { get; } = new PlayerMovementRequestOutcome(PlayerMovementRequestStatus.RunIsNotPlaying, default);
        public static PlayerMovementRequestOutcome NoMovement { get; } = new PlayerMovementRequestOutcome(PlayerMovementRequestStatus.NoMovement, default);
        public static PlayerMovementRequestOutcome PositionUnchanged { get; } = new PlayerMovementRequestOutcome(PlayerMovementRequestStatus.PositionUnchanged, default);

        public PlayerMovementRequestStatus Status { get; private set; }

        public PlayerMovementRequest Request { get; private set; }

        public bool HasRequest => Status == PlayerMovementRequestStatus.Requested;

        internal void Fill(PlayerMovementRequestStatus status, PlayerMovementRequest request)
        {
            Validate(status, request);
            Status = status;
            Request = request;
        }

        public static PlayerMovementRequestOutcome Requested(
            PlayerMovementRequest request)
        {
            return new PlayerMovementRequestOutcome(PlayerMovementRequestStatus.Requested, request);
        }

        private static void Validate(
            PlayerMovementRequestStatus status,
            PlayerMovementRequest request)
        {
            if (status != PlayerMovementRequestStatus.Requested)
            {
                return;
            }

            if (request.Correlation.ArenaRunId.IsNone)
            {
                throw new ArgumentException(nameof(request));
            }

            if (request.Correlation.InteractionId.IsNone)
            {
                throw new ArgumentException(nameof(request));
            }
        }
    }
}

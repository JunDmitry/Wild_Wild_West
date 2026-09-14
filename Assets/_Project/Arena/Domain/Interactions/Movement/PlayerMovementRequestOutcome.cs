using System;

namespace Game.Arena.Domain.Interactions.Movement
{
    public sealed class PlayerMovementRequestOutcome
    {
        private PlayerMovementRequestOutcome(
            PlayerMovementRequestStatus status,
            PlayerMovementRequest request)
        {
            if (status == PlayerMovementRequestStatus.Requested)
            {
                if (request.Correlation.ArenaRunId.IsNone)
                {
                    throw new ArgumentException(nameof(request));
                }

                if (request.Correlation.InteractionId.IsNone)
                {
                    throw new ArgumentException(nameof(request));
                }
            }

            Status = status;
            Request = request;
        }
        public static PlayerMovementRequestOutcome NotPlaying { get; } = new PlayerMovementRequestOutcome(PlayerMovementRequestStatus.RunIsNotPlaying, default);
        public static PlayerMovementRequestOutcome NoMovement { get; } = new PlayerMovementRequestOutcome(PlayerMovementRequestStatus.NoMovement, default);
        public static PlayerMovementRequestOutcome PositionUnchanged { get; } = new PlayerMovementRequestOutcome(PlayerMovementRequestStatus.PositionUnchanged, default);

        public PlayerMovementRequestStatus Status { get; }

        public PlayerMovementRequest Request { get; }

        public bool HasRequest => Status == PlayerMovementRequestStatus.Requested;

        public static PlayerMovementRequestOutcome Requested(
            PlayerMovementRequest request)
        {
            return new PlayerMovementRequestOutcome(PlayerMovementRequestStatus.Requested, request);
        }
    }
}

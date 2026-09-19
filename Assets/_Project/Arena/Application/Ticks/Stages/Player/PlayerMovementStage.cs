using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class PlayerMovementStage
    {
        private readonly IPlayerMovementResolver _resolver;
        private readonly PendingInteractionTracker _interactionTracker;

        public PlayerMovementStage(IPlayerMovementResolver resolver, PendingInteractionTracker interactionTracker)
        {
            _resolver = resolver ?? throw new System.ArgumentNullException(nameof(resolver));
            _interactionTracker = interactionTracker ?? throw new System.ArgumentNullException(nameof(interactionTracker));
        }

        public StageExecutionStatus Execute(
            ArenaRun run,
            PlayerFrameInput input,
            GameDuration delta,
            ArenaRunTickRecorder recorder)
        {
            recorder.RecordStage(ArenaRunTickStage.PlayerMovement);

            PlayerMovementRequestOutcome requestOutcome = run.RequestPlayerMovement(input.Movement, delta);

            if (requestOutcome.HasRequest == false)
            {
                return StageExecutionStatus.Completed;
            }

            PlayerMovementRequest request = requestOutcome.Request;
            _interactionTracker.Track(request.Correlation);

            Position3D acceptedPosition = _resolver.Resolve(request);
            PlayerMovementResolution movementResolution = new(request.Correlation, acceptedPosition);
            PlayerMovementResolutionOutcome resolutionOutcome = run.ApplyPlayerMovement(movementResolution);

            recorder.RecordChange(resolutionOutcome.Change);

            if (resolutionOutcome.IsAccepted == false)
            {
                return StageExecutionStatus.InteractionLeftPending;
            }

            _interactionTracker.Clear();

            return StageExecutionStatus.Completed;
        }
    }
}

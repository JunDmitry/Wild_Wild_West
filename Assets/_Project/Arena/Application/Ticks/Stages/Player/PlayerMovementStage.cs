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

        public PlayerMovementStage(IPlayerMovementResolver resolver)
        {
            _resolver = resolver ?? throw new System.ArgumentNullException(nameof(resolver));
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
            Position3D acceptedPosition = _resolver.Resolve(request);
            PlayerMovementResolution movementResolution = new(request.Correlation, acceptedPosition);
            PlayerMovementResolutionOutcome resolutionOutcome = run.ApplyPlayerMovement(movementResolution);

            recorder.RecordChange(resolutionOutcome.Change);

            if (resolutionOutcome.IsAccepted == false)
            {
                return StageExecutionStatus.InteractionLeftPending;
            }

            return StageExecutionStatus.Completed;
        }
    }
}

using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Interactions;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class PendingInteractionRecoveryStage
    {
        private readonly PendingInteractionTracker _tracker;

        public PendingInteractionRecoveryStage(PendingInteractionTracker tracker)
        {
            _tracker = tracker ?? throw new System.ArgumentNullException(nameof(tracker));
        }

        public StageExecutionStatus Execute(
            ArenaRun run,
            ArenaRunTickRecorder recorder)
        {
            if (run.HasPendingInteraction == false)
            {
                _tracker.Clear();
                return StageExecutionStatus.Completed;
            }

            if (_tracker.HasCorrelation == false)
            {
                throw new System.InvalidOperationException("ArenaRun has a pending interaction without a tracked correlation.");
            }

            recorder.RecordStage(ArenaRunTickStage.PendingInteractionCancellation);
            InteractionCancellationOutcome outcome = run.CancelPendingInteraction(_tracker.Correlation, InteractionCancellationReason.SupersededByLifecycle);

            if (outcome.IsCancelled == false)
            {
                throw new System.InvalidOperationException("Failed to cancel the pending interaction: " + outcome.Status);
            }

            _tracker.Clear();

            return StageExecutionStatus.Completed;
        }
    }
}

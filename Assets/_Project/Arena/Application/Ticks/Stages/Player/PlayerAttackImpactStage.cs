using System.Collections.Generic;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Attack;

namespace Game.Arena.Application.Ticks.Stages
{
    internal sealed class PlayerAttackImpactStage
    {
        private readonly IPlayerAttackTargetingResolver _resolver;

        public PlayerAttackImpactStage(IPlayerAttackTargetingResolver resolver)
        {
            _resolver = resolver ?? throw new System.ArgumentNullException(nameof(resolver));
        }

        public StageExecutionStatus Execute(
            ArenaRun run,
            PlayerFrameInput input,
            ArenaRunTickRecorder recorder)
        {
            recorder.RecordStage(ArenaRunTickStage.PlayerAttackImpact);

            PlayerAttackImpactRequestOutcome impactRequestOutcome = run.RequestPlayerAttackImpact(input.AimDirection);

            if (impactRequestOutcome.HasRequest == false)
            {
                return StageExecutionStatus.Completed;
            }

            PlayerAttackImpactRequest request = impactRequestOutcome.Request;
            IReadOnlyList<EnemyId> hitEnemies = _resolver.Resolve(request);

            if (hitEnemies == null)
            {
                throw new System.InvalidOperationException("Attack targeting resolver returned null.");
            }

            PlayerAttackImpactResolution attackImpactResolution = new(request.Correlation, hitEnemies);
            PlayerAttackImpactOutcome outcome = run.ApplyPlayerAttackImpact(attackImpactResolution);

            recorder.RecordChange(outcome.Change);

            if (outcome.IsAccepted == false)
            {
                return StageExecutionStatus.InteractionLeftPending;
            }

            return StageExecutionStatus.Completed;
        }
    }
}

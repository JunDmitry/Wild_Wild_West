using System;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Movement;
using Game.Arena.Domain.Time;

namespace Game.Arena.Application.Tests
{
    internal static class ArenaRunExtensions
    {
        private readonly static Random s_random = new();

        public static void MakeRunDefeat(this ArenaRun run, GameDuration enemyAttackWindup = default, int reqularsToSpawn = default, params Position3D[] spawnPositions)
        {
            if (enemyAttackWindup == default)
            {
                enemyAttackWindup = new GameDuration(0.2d);
            }

            if (reqularsToSpawn == default)
            {
                reqularsToSpawn = run.RegularEnemiesRemainingToSpawn;
            }

            int settedSpawnPositionsCount = spawnPositions == null ? 0 : spawnPositions.Length;

            for (uint i = 0; i < reqularsToSpawn; i++)
            {
                Position3D spawnPosition;

                if (i >= settedSpawnPositionsCount)
                {
                    float x = s_random.Next(0, 2) == 0 ? run.ArenaBounds.MinimumX - 1 : run.ArenaBounds.MaximumX + 1;
                    float z = s_random.Next(0, 2) == 0 ? run.ArenaBounds.MinimumZ - 1 : run.ArenaBounds.MaximumZ + 1;
                    spawnPosition = new(x, run.ArenaBounds.GroundY, z);
                }
                else
                {
                    spawnPosition = spawnPositions[i];
                }

                Domain.Interactions.Spawn.EnemySpawnRequestOutcome spawnRequest = run.RequestEnemySpawn();

                if (spawnRequest.HasRequest == false)
                {
                    break;
                }

                Domain.Interactions.Spawn.EnemySpawnResolutionOutcome outcome = run.ApplyEnemySpawn(new(spawnRequest.Request.Correlation, EnemyId.FromValue(10000 + i), spawnPosition));

                if (outcome.IsSpawned == false)
                {
                    run.CancelPendingInteraction(spawnRequest.Request.Correlation, Domain.Interactions.InteractionCancellationReason.SupersededByLifecycle);
                    break;
                }
            }

            int infinityCycleGuard = 10000;

            while (infinityCycleGuard > 0 && run.PlayerHealth.IsDepleted == false)
            {
                Domain.Interactions.Movement.EnemyMovementBatchRequestOutcome reqOutcome = run.RequestEnemyMovementBatch(new GameDuration(1d));

                if (reqOutcome.HasRequest)
                {
                    Domain.Interactions.Movement.EnemyMovementBatchRequest req = reqOutcome.Request;

                    run.ApplyEnemyMovementBatch(new EnemyMovementBatchResolution(
                        req.Correlation,
                        new EnemyMovementBatchResolutionEntry[]
                        {
                            new(req.Intents[0].EnemyId, req.Intents[0].Intent.RequestedPosition)
                        }));
                }

                run.StartEligibleEnemyAttacks();
                run.AdvanceTime(enemyAttackWindup);
                run.ResolveDueEnemyAttackImpacts();
                infinityCycleGuard--;
            }
        }
    }
}

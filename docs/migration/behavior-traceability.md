# Arena Combat Behavior Traceability

This document maps the intended gameplay behavior to legacy characterization coverage, known legacy defects, Arena Run aggregate invariants, and the target IDDD test suite.

Legacy characterization tests preserve correct existing behavior.

Legacy defect tests reproduce known incorrect behavior and must not be treated as target requirements.

Target Arena Run tests define the intended behavior of the Arena Combat bounded context.

| Behavior | Intended behavior | Aggregate invariants | Legacy characterization | Legacy defect | Future Arena Run / Application test |
|---|---|---|---|---|---|
| B-001 New run | A new Arena Run starts with one Player at full Health, Ranged Weapon selected, Wave 1 active in Regular Combat, and no active Enemies. | INV-001, INV-002, INV-010, INV-030, INV-031, INV-050 | `NewGameStartsWithFirstRegularCombatWave` | — | `StartCreatesPlayingRunAtFirstWave` |
| B-002 Movement | Player movement is expressed on the Arena XZ plane, does not directly modify vertical position, respects Arena boundaries, and uses an external Movement Resolution for collision-dependent movement. | INV-013, INV-054, INV-071, INV-072, INV-073, INV-074, INV-075 | `UnblockedMovementChangesPlayerPosition`, `BlockedMovementPreservesPlayerPosition` | LEG-020 | `ResolvedMovementUpdatesPlayerPosition`, `MovementCannotCrossArenaBoundary` |
| B-003 Weapon switch | Switching changes the selected Weapon between Ranged and Melee while the Player is allowed to act. | INV-013, INV-015, INV-044, INV-053, INV-078 | Existing legacy weapon rule tests | — | `SwitchWeaponChangesSelectedWeapon`, `NewRunSelectsRangedWeapon`, `SwitchWeaponIsRejectedWhileInteractionIsPending` |
| B-004 Q + LMB | When weapon switching and attacking are requested during the same gameplay step, the weapon switch is applied before planning and executing the attack. | INV-013, INV-054, INV-061, INV-062, INV-063, INV-072, INV-073 | — | LEG-001, LEG-002, LEG-024 | `SwitchAndAttackUsesNewlySelectedWeapon` |
| B-005 Ranged attack | A ready Ranged Attack produces an external three-dimensional attack interaction, applies a matching resolution only once, damages a valid Enemy in the same Arena Run, and advances ranged readiness. | INV-020, INV-060, INV-061, INV-062, INV-064, INV-072, INV-073, INV-074, INV-075 | `RangedAttackDamagesResolvedEnemy` | LEG-022 | `ResolvedRangedAttackDamagesTarget`, `StaleRangedAttackResolutionIsRejected`, `RangedImpactDamagesResolvedEnemy`, `MeleeAttackDamagesEachEnemyOnce`, `TargetBeyondWeaponRangeIsRejected` |
| B-006 Melee attack | A ready Melee Attack uses the Player position after resolved movement, affects each valid Enemy at most once, and advances melee readiness. | INV-020, INV-024, INV-072, INV-073, INV-074, INV-075, INV-110, INV-111, INV-112, INV-114 | — | LEG-006, LEG-019, LEG-022 | `MeleeAttackUsesResolvedPlayerPosition`, `MeleeAttackDamagesEachEnemyOnce`, `RangedImpactDamagesResolvedEnemy`, `MeleeAttackDamagesEachEnemyOnce`, `TargetBeyondWeaponRangeIsRejected` |
| B-007 Enemy defeat | Lethal damage removes the Enemy from the active Enemy collection and records an EnemyDefeated domain fact. Defeating a Boss updates the current Wave boss progression. | INV-003, INV-020, INV-021, INV-022, INV-023, INV-033, INV-034, INV-035, INV-130, INV-131, INV-132 | Covered by `DefeatedFinalBossChangesGamePhaseToVictory` and legacy combat tests | — | `LethalDamageRemovesEnemyAndRaisesEnemyDefeated`, `LethalBossDamageMarksBossDefeated`, `LethalDamageRemovesEnemyAndRaisesEnemyDefeated` |
| B-008 Wave transition | A Boss cannot become active until every required Regular Enemy has been spawned and defeated. Defeating the Boss completes the current Wave. A non-final completed Wave starts the next Wave. | INV-030, INV-031, INV-032, INV-033, INV-034, INV-035, INV-036, INV-100, INV-101, INV-102, INV-103, INV-104, INV-105, INV-106 | Existing legacy wave rule tests | LEG-003, LEG-004, LEG-005, LEG-017, LEG-021 | `DefeatingAllRegularEnemiesRequestsBossSpawn`, `DefeatingBossCompletesWave`, `CompletingNonFinalWaveStartsNextWave`, `SpawnRequestIsRegularWhileRegularEnemiesRemain`, `WaveWithoutRegularEnemiesStartsInBossCombatAndRequestsBoss`, `BossSpawnsOnceThenNoSpawnIsDue`, `DefeatingLastRegularEnemyEntersBossCombat`, `DefeatingBossCompletesWave`, `CompletingNonFinalWaveStartsNextWave`, `DefeatingLastRegularEnemyRaisesWavePhaseChangedToBossCombat` |
| B-009 Victory | Completion of the final Wave transitions Arena Run to Victory. Victory is terminal and further combat progression is rejected. | INV-035, INV-037, INV-051, INV-053, INV-054, INV-130, INV-131, INV-132 | `DefeatedFinalBossChangesGamePhaseToVictory` | — | `DefeatingFinalBossCompletesRun`, `VictoriousRunRejectsFurtherCombatProgression`, `DefeatingFinalBossCompletesRun`, `FinalBossImpactProducesWaveAndVictoryEventsInOneRevision`, `VictoriousRunRejectsFurtherCombatProgression` |
| B-010 Defeat | Player Health reaching zero transitions Arena Run to Defeat. Defeat is terminal and further combat progression is rejected. | INV-012, INV-013, INV-051, INV-052, INV-053, INV-054, INV-130, INV-131, INV-132 | `PlayerDefeatChangesGamePhase` | LEG-009 | `LethalPlayerDamageDefeatsRun`, `DefeatedRunRejectsFurtherCombatProgression` |
| B-011 Restart | Restart is an Application lifecycle operation. After the configured delay, the Arena scene is reloaded and a new Arena Run with new ArenaRunId and PlayerId is created at Wave 1. The terminal Arena Run is not reset. | INV-001, INV-002, INV-031, INV-051, INV-055, INV-070 | `DefeatRequestsRestartAfterConfiguredDelay` | — | `DefeatDelayRequestsArenaReload`, `ReloadCreatesNewArenaRunAtFirstWave` |

## Interaction Correlation Coverage

Interaction correlation is a target architecture requirement and has no equivalent guarantee in the legacy implementation.

Every external interaction request carries:

- ArenaRunId
- InteractionId
- AggregateRevision

The Arena Run validates all three values before accepting a resolution.

The following target tests are mandatory:

| Requirement | Future test |
|---|---|
| Resolution belongs to another Arena Run | `ResolutionForAnotherArenaRunIsRejected` |
| Resolution has an unknown InteractionId | `UnknownInteractionResolutionIsRejected` |
| Resolution belongs to an older AggregateRevision | `StaleInteractionResolutionIsRejected` |
| The same resolution is delivered twice | `DuplicateInteractionResolutionIsRejected` |
| A rejected resolution changes aggregate state | `RejectedInteractionResolutionDoesNotChangeState` |
| A rejected resolution advances AggregateRevision | `RejectedInteractionResolutionDoesNotAdvanceRevision` |
| An accepted state-changing resolution advances AggregateRevision | `AcceptedStateChangingResolutionAdvancesRevision` |
| An accepted resolution without state change does not advance AggregateRevision | `AcceptedResolutionWithoutMovementDoesNotAdvanceRevision` |
| An interaction request is created without state change | `CreatingInteractionRequestDoesNotAdvanceRevision` |
| Resolution type does not match pending interaction | `MismatchedInteractionResolutionIsRejected` |
| Enemy movement batch resolution contains an unknown EnemyId | `EnemyMovementBatchWithUnknownEnemyIsRejected` |
| An interaction is opened while another is pending | `OpenWhilePendingThrows` |
| A cancelled interaction receives a resolution | `CancelledInteractionResolutionIsRejected` |
| A resolution carries the opening revision but the aggregate has advanced | `StaleInteractionResolutionIsRejected` |
| A resolution carries an older revision than the opening one | `ResolutionCarryingOldRevisionIsRejected` |
| A resolution with an invalid payload closes the pending interaction | `RejectedPayloadResolutionDoesNotClosePendingInteraction` |
| A valid resolution follows a rejected payload resolution | `ValidResolutionCanBeAppliedAfterRejectedPayloadResolution` |
| An accepted position extends the requested movement | `AcceptedPositionBeyondRequestedDistanceIsRejected` |
| An accepted position lies behind the movement origin | `AcceptedPositionBehindRequestedDirectionIsRejected` |
| An accepted position leaves the movement path | `AcceptedPositionOutsideRequestedMovementRayIsRejected` |
| An accepted position shortens the requested movement | `AcceptedPositionAtPartialRequestedDistanceIsApplied` |
| Cancellation is attempted with a mismatched correlation | `CancelWithMismatchedCorrelationKeepsPending` |
| A state-changing command is issued while an interaction is pending | `SwitchWeaponIsRejectedWhileInteractionIsPending` |
| A state-changing command is issued after the pending interaction is resolved | `SwitchWeaponIsAllowedAfterPendingInteractionIsResolved` |
| A resolution of another interaction kind is applied to a pending spawn | `MovementResolutionIsRejectedForPendingSpawnInteraction` |
| A spawn resolution carries a None or duplicate EnemyId | `SpawnResolutionWithNoneEnemyIdIsRejectedWithoutSideEffects`, `DuplicateEnemyIdIsRejected` |
| Game Time is advanced while an interaction is pending | `AdvanceTimeIsRejectedWhileInteractionIsPending` |
| Starting a Player attack does not damage enemies | `StartingPlayerAttackDoesNotDamageEnemies` |
| Player attack start advances revision and raises event | `StartingPlayerAttackAdvancesRevisionOnce`, `StartingPlayerAttackProducesPlayerAttackStartedEvent` |
| Cooldown starts at Attack Start | `RangedAttackUsesCooldownAtStart` |
| Weapon switching during pending attack is rejected | `SwitchWeaponIsRejectedWhileAttackPending` |
| Completing a non-final wave starts the next wave in the same aggregate operation | `CompletingNonFinalWaveStartsNextWave` |
| Completing the final wave makes the Arena Run victorious | `DefeatingFinalBossCompletesRun` |
| Events caused by one aggregate operation share one revision | `FinalBossImpactProducesWaveAndVictoryEventsInOneRevision` |

## Legacy Defect Migration Rule

A legacy defect is considered resolved by migration only when:

1. The corresponding target behavior is implemented.
2. The target test is green.
3. The production flow uses the Arena Combat implementation for that behavior.
4. The legacy implementation is no longer used for that production path.
5. The defect register status is changed to `Resolved by migration`.

Legacy defect reproduction tests remain unchanged until their legacy production component is retired.

# Arena Combat Behavior Traceability

| Behavior | Target status | Target tests |
|---|---|---|
| B-001 New Arena Run | Implemented in Domain | `StartCreatesPlayingRunAtFirstWave`, `NewRunSelectsRangedWeapon` |
| B-002 Player Movement | Implemented in Domain | `ResolvedMovementUpdatesPlayerPosition`, `MovementCannotCrossArenaBoundary`, `AcceptedPositionOffGroundPlaneIsRejected` |
| B-003 Weapon Switching | Implemented in Domain | `SwitchWeaponChangesSelectedWeapon`, `SwitchWeaponIsRejectedWhileInteractionIsPending`, `SwitchWeaponIsRejectedWhileAttackIsPending` |
| B-004 Switch and Attack | Partially implemented; Application command order pending | `SwitchWeaponChangesSelectedWeapon`, `StartingPlayerAttackProducesPlayerAttackStartedEvent` |
| B-005 Ranged Attack | Implemented in Domain | `RangedImpactDamagesResolvedEnemy`, `TargetBeyondWeaponRangeIsRejected`, `DuplicateImpactResolutionIsRejected` |
| B-006 Melee Attack | Implemented in Domain | `MeleeAttackDamagesEachEnemyOnce`, `TargetBeyondWeaponRangeIsRejected` |
| B-007 Enemy Defeat | Implemented in Domain | `LethalDamageRemovesEnemyAndRaisesEnemyDefeated`, `DefeatedEnemyPendingAttackIsCancelledByPlayerImpact` |
| B-008 Wave Progression | Implemented in Domain | `DefeatingLastRegularEnemyRaisesWavePhaseChangedToBossCombat`, `DefeatingBossCompletesWave`, `CompletingNonFinalWaveStartsNextWave` |
| B-009 Victory | Implemented in Domain | `DefeatingFinalBossCompletesRun`, `VictoriousRunRejectsFurtherCombatProgression` |
| B-010 Defeat | Implemented in Domain | `LethalEnemyAttackDefeatsPlayer`, `LethalEnemyAttackDefeatsArenaRun`, `DefeatedRunRejectsFurtherCombatProgression` |
| B-011 Restart | Planned in Application | `RestartOfDefeatedRunAllocatesNewIdentities`, `RestartOfDefeatedRunRemovesOldAggregate`, `RestartOfDefeatedRunAddsNewAggregate`, `RestartOfDefeatedRunChangesActiveArenaRunId`, `ReloadCreatesNewArenaRunAtFirstWave` |

## Interaction Correlation Coverage

| Requirement | Target test |
|---|---|
| Resolution for another ArenaRun is rejected | `ResolutionForAnotherArenaRunIsRejected` |
| Unknown InteractionId is rejected | `UnknownInteractionResolutionIsRejected` |
| Stale AggregateRevision is rejected | `StaleInteractionResolutionIsRejected` |
| Duplicate resolution is rejected | `DuplicateInteractionResolutionIsRejected` |
| Rejected resolution does not change state | `RejectedInteractionResolutionDoesNotChangeState` |
| Rejected resolution does not advance revision | `RejectedInteractionResolutionDoesNotAdvanceRevision` |
| Request creation does not advance revision | `CreatingInteractionRequestDoesNotAdvanceRevision` |
| Valid movement resolution can shorten movement | `AcceptedPositionAtPartialRequestedDistanceIsApplied` |
| Invalid payload keeps interaction pending | `RejectedPayloadResolutionDoesNotClosePendingInteraction` |
| Enemy movement batch is atomic | `EnemyMovementBatchResolutionMovesAllEnemiesAtomically` |
| Enemy movement batch rejects missing EnemyId | `EnemyMovementBatchWithMissingEnemyIdIsRejected` |
| Enemy movement batch rejects duplicate EnemyId | `EnemyMovementBatchWithDuplicateEnemyIdIsRejected` |
| Enemy movement batch has no Domain Events | `EnemyMovementBatchDoesNotProduceDomainEvents` |
| Player attack impact rejects unknown target | `UnknownTargetIsRejectedWithoutSideEffects` |
| Player attack impact collapses melee duplicates | `MeleeAttackDamagesEachEnemyOnce` |
| Enemy attacks process in EnemyId order | `DueEnemyAttacksResolveInEnemyIdOrder` |
| Enemy attack batch advances revision once | `MultipleEnemyImpactsAdvanceRevisionOnce` |
| Defeat cancels remaining attacks | `LethalEnemyAttackCancelsRemainingEnemyAttacks`, `LethalEnemyAttackCancelsPendingPlayerAttack` |

## Legacy Defect Migration Rule

A legacy defect is resolved by migration only when:

1. The target behavior is implemented.
2. Target tests are green.
3. Production flow uses the Arena Combat implementation.
4. Legacy production code is no longer used for that behavior.
5. The defect register status is changed to Resolved by migration.
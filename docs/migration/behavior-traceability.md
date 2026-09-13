# Trace matrix

| Behavior |	Legacy characterization |	Legacy defect |	Future ArenaRun test |
|---|---|---|---|
| B-001 New run |	NewGameStartsWithFirstRegularCombatWave |	— |	StartCreatesPlayingRunAtFirstWave |
| B-002 Movement |	UnblockedMovementChangesPlayerPosition |	LEG-020 |	ResolvedMovementUpdatesPlayerPosition |
| B-003 Weapon switch |	Existing legacy unit tests |	— |	SwitchWeaponChangesSelectedWeapon |
| B-004 Q + LMB |	— |	ExistingBehaviorSwitchAndAttackUsesInconsistentWeaponStates |	SwitchAndAttackUsesNewlySelectedWeapon |
| B-005 Ranged attack |	RangedAttackDamagesResolvedEnemy |	— |	ResolvedRangedAttackDamagesTarget |
| B-006 Melee attack |	— |	ExistingBehaviorDuplicateMeleeHitsDamageEnemyMoreThanOnce |	MeleeAttackDamagesEachEnemyOnce |
| B-007 Enemy defeat |	Covered through final boss scenario |	— |	LethalDamageRemovesEnemyAndRaisesEnemyDefeated |
| B-008 Wave transition |	Existing rule tests |	LEG-003, LEG-004, LEG-005 |	DefeatingAllRegularEnemiesRequestsBossSpawn |
| B-009 Victory |	DefeatedFinalBossChangesGamePhaseToVictory |	— |	DefeatingFinalBossCompletesRun |
| B-010 Defeat |	PlayerDefeatChangesGamePhase |	LEG-009 |	LethalPlayerDamageDefeatsRun |
| B-011 Restart |	DefeatRequestsRestartAfterConfiguredDelay |	— |	Application lifecycle test |

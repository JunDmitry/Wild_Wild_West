# Legacy Defect Register

> Statuses refreshed 2026-10-25 by `architect-role` as an exception: `qa-role` is not operating yet. Rows whose target work landed in T-3–T-7 now say so; every remaining row waits on the T-9 cut-over or T-10 removal, not on target implementation.

| ID | Component | Problem | Target status |
|---|---|---|---|
| LEG-001 | Simulation.Execute | Weapon switch result is ignored by subsequent operations. | Implemented in target; production cut-over pending |
| LEG-002 | SimulationPlanner and WeaponRules | Q plus LMB uses inconsistent weapon state. | Implemented in target; production cut-over pending |
| LEG-003 | WaveRules.TickRegular | EnemySpawned fact may be emitted without a spawned Enemy. | Implemented in target; production cut-over pending |
| LEG-004 | WaveRules.TickBoss | EnemySpawned fact may be emitted without a spawned Boss. | Implemented in target; production cut-over pending |
| LEG-005 | WaveRules.Tick | Completed wave can return invalid default outcome. | Implemented in target; production cut-over pending |
| LEG-006 | SimulationPlan | Melee query is created before Player movement resolution. | Implemented in target interaction protocol; production cut-over pending |
| LEG-007 | SimulationPlan | Spawn is planned before final defeat state is known. | Implemented in target; production cut-over pending |
| LEG-008 | EnemyRules | Enemy movement ignores Unity 3D collision and navigation constraints. | Implemented in target; production cut-over pending |
| LEG-009 | EnemyRules | Remaining Enemy damage is not explicitly stopped after Player death. | Implemented in target; production cut-over pending |
| LEG-010 | GameState | IReadOnlyDictionary does not provide deep immutability. | Implemented in target; production cut-over pending |
| LEG-011 | GameConfig | IReadOnlyList over array allows mutable aliases. | Implemented in target; production cut-over pending |
| LEG-012 | SimulationResult | Boolean-oriented result encoding. | Implemented in target; production cut-over pending |
| LEG-013 | SimulationResult | Result collections can expose mutable aliases. | Implemented in target; production cut-over pending |
| LEG-014 | AttackResolution | No request-resolution correlation. | Implemented in target; production cut-over pending |
| LEG-015 | FrameInput and FrameContext | Application must guess Domain external query requirements. | Implemented in target; production cut-over pending |
| LEG-016 | GameConfig | HasBoss permits bossless waves. | Implemented in target; production cut-over pending |
| LEG-017 | WaveRules | One spawn per tick is an implicit policy. | Implemented in target; production cut-over pending |
| LEG-018 | EntityId | Untyped EntityId does not express ArenaRun, Player, or Enemy identity. | Implemented in target; legacy removal pending |
| LEG-019 | WeaponRules.TryPlayerAttack | Duplicate melee EnemyId damages target multiple times. | Implemented in target; production cut-over pending |
| LEG-020 | MovementRules.ClampToArena | Player radius is ignored by arena boundary calculation. | Implemented in target; production cut-over pending |
| LEG-021 | WaveRules | Spawn resolution is not correlated with spawn request kind. | Implemented in target; production cut-over pending |
| LEG-022 | WeaponRules | Attack resolution is not correlated with attack request. | Implemented in target; production cut-over pending |
| LEG-023 | GameState | Enemy collection can be mutated through implementation type. | Implemented in target; production cut-over pending |
| LEG-024 | Simulation.Execute | State and SimulationResult can contradict each other. | Implemented in target; production cut-over pending |
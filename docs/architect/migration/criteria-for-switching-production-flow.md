# Production Flow Cut-Over Criteria

## Functional Equivalence

- B-001 through B-011 are covered by target tests.
- Every legacy defect has a documented target status.
- Every defect marked as implemented in target has corresponding green tests.
- Application restart lifecycle covers B-011.
- Unity adapters support player movement, ranged targeting, melee targeting, enemy movement, spawning, defeat, and victory.

## Quality

- Architecture tests are green.
- Domain tests are green.
- Application tests are green.
- Infrastructure tests are green.
- Presentation tests are green.
- ArenaSandbox PlayMode smoke tests are green.

## Required ArenaSandbox Smoke Scenarios

- Start → Wave 1 → Boss → Wave 2 → Wave 3 → Victory.
- Start → Player damage → Defeat → delay → ArenaScene reload → Wave 1.
- Player movement against Arena boundaries.
- Ranged attack hit and miss.
- Melee attack against multiple Enemies.
- Enemy movement toward Player.
- Enemy windup followed by Player evasion.
- Enemy windup followed by Player defeat.

### Acceptance detail — the three scenarios added in T-9

The seven-scenario T-8.6 fixture covers defeat + reload (2), arena-bound movement (3), ranged hit and miss (4), melee against multiple enemies (5), and a two-wave start → boss → victory chain (part of 1). Three remained; all three were added on 2026-10-26 (see Status below). All three are test-side work in `Game.CompositionRoot.PlayMode.Tests`, using the existing `ArenaSceneTestRig`, `FakeInputDevice`, and `ArenaGameConfiguration` values; none requires a production, scene, prefab, assembly-definition, or package change. Configuration values may be tuned for determinism; the observables below are fixed.

**Scenario 1 — Start → Wave 1 → Boss → Wave 2 → Wave 3 → Victory**

- Setup: production arena, player, weapons, enemies, and a `WaveCatalog` of three waves. Use boss-only waves (`RegularEnemyCount = 0`) so each wave is the deterministic boss phase, mirroring the existing two-wave scenario; regular spawning and timing are already proven elsewhere.
- Drive: enter the arena and wait for the active run; for each wave wait for the boss to spawn, register and sync its collider, aim at it, and strike whenever the selected weapon is ready, until the run advances.
- Observable: the HUD wave text reads `Wave 1 of 3`, `Wave 2 of 3`, then `Wave 3 of 3`, in that order; the run stays `Playing` while waves advance; after the third boss is defeated the status is `Victory`, the victory window is shown once, and the reload count stays zero.

**Scenario 7 — Enemy windup followed by Player evasion**

- Setup: one regular enemy with a long windup (at least 1.5 s), a short attack range, lethal or near-lethal damage, and a slow movement speed; an arena large enough for the player to leave that range.
- Drive: enter the arena and wait until the enemy's pending attack is observable (`EnemyStateSnapshot.HasPendingAttack` / the read model's `HasPendingAttack`); then move the player out of the enemy's attack range before the windup ends.
- Observable: the player's health is unchanged when the windup starts; after the attack resolves, the player's health still equals its pre-windup value, the run status is still `Playing`, and the enemy has no pending attack.

**Scenario 8 — Enemy windup followed by Player defeat**

- Setup: one regular enemy with a long windup, lethal damage, and a short attack range; no movement or attack input from the player.
- Drive: enter the arena and wait for the windup to start, then let it complete in range.
- Observable: the player's health is unchanged at windup start; after the impact the player's health is depleted, the run status is `Defeat`, and the defeat window is shown for that run. Reload timing is already covered by scenario 2; this scenario asserts the defeat, and may additionally assert zero reloads on the defeated tick.

**Status (2026-10-26):** all three scenarios are implemented and green in `ArenaSandboxSmokeTests` — `ThreeWaveStart_ReachesVictory_WithWaveTextInOrder`, `EnemyWindup_PlayerEscapes_ResolvesAsMiss`, and `EnemyWindup_PlayerStaysInRange_DefeatsTheRun`; the fixture ran 10/10 in batchmode, exit 0 (commit `f109da0`).

## Performance

Before T-9, define measurable thresholds for:

- average frame time;
- worst frame time;
- GC allocations per frame;
- maximum active Enemy count;
- physics query count per frame.

The target implementation must not exceed agreed thresholds compared with legacy without an accepted performance ADR.

## Cut-Over Procedure

1. Switch the main gameplay scene to the new CompositionRoot.
2. Keep legacy code for one release cycle.
3. Monitor errors, performance, and behavior regressions.
4. Execute T-10 only after validation.
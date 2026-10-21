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
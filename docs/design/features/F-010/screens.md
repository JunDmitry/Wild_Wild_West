# Screens

Feature F-010 — Defeat and scene restart.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-003 | Defeat window | tell the player the run ended in defeat and name the wave reached, inside the automatic restart delay, without offering any control | FL-048, FL-049 |
| S-001 | Arena gameplay view | show the frozen arena at the moment of defeat, then the fresh run after the restart — as F-001, F-005, and F-008 already define | FL-048, FL-050 |
| S-002 | Victory window | show the end of a completed run, unchanged from F-011 | FL-051 |

## Notes

- `S-003` is new and is the only screen F-010 mints. `S-001` is F-001's, and `S-002` is F-011's; F-010 extends neither.
- `S-003` is distinct from `S-002`: the Victory window marks the end of a completed run and is reached from the third wave's boss; the Defeat window marks a failed run and is reached from the player's health reaching zero. They share no control, because neither has one, and they never follow one another.
- `S-003`'s single job is to say what happened and where the run ended. It carries no score, no statistics, no rewards, and no restart control; those are out of scope.
- `S-003` lives only for the restart delay. The delay is a tuning value; the screen is replaced by the fresh run's `S-001` without input.
- `S-001` reaches `S-003` from its terminal defeat state, which F-001 and F-001b already design: the view freezes and the inputs are inert.
- The message's wording is product's copy; this design fixes its structure — a defeat statement and the wave reached — not its words.

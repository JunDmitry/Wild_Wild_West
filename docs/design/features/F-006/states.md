# States

Feature F-006 — Enemy spawning outside the arena, unseen.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-001 enemy entry | no Spawn is due — no enemy is entering | a Spawn is due and the enemy is placed outside the view and moving inward; deliberately invisible — no placeholder, effect, or announcement before entry | — (an ineligible position is silently replaced by another; no error is shown) | run terminal — Defeat or Victory; no further Spawn is processed | an enemy has crossed Arena Bounds — Active, visible, alive, and counted toward the Wave | a Wave is entering over time — some enemies Active, others still due or moving inward; the Wave stays open |
| S-001 observation of entry | no entering enemy is in view | — (the view has no loading state; the pre-entry enemy is outside it by design) | — (entry produces no error state in the view) | run terminal — no further enemy enters | an enemy is visible moving inward after crossing the boundary | an enemy is crossing the boundary — its first visible moment; it is seen moving in, never appearing |
| S-001 gameplay inputs | no input this step — nothing changes | — (inputs have no loading state) | — (no error state; an input changes nothing or acts, silently) | run terminal — inputs do not affect the run; the exact refusal is F-005's and F-010's | run Playing — inputs act as F-001, F-002, F-003, and F-004 define; entry does not change them | an attack is pending — F-002's rule applies unchanged; entry does not change it |

## Notes

- The `loading` cell for entry is the Spawned state: the enemy exists outside the view and is moving inward. It is invisible by requirement, so no placeholder or effect may be shown; the state is listed to prevent one being added.
- An entered enemy is alive and counts toward the active Wave. A defeated enemy stops counting (F-007 and F-009 own defeat).
- The run's terminal state disables entry; entered enemies remain where the terminal state leaves them. A restart is a new run with its own Spawn Pacing (F-010).
- An ineligible spawn position is not an error: it is rejected before the enemy appears and another position is chosen. No state is shown for it.
- No state exists in which an enemy is visible while still being placed; visibility begins only at the boundary crossing.

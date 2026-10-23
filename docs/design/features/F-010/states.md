# States

Feature F-010 — Defeat and scene restart.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-003 defeat window | the run is Playing — no defeat window | the window is showing; the restart delay is elapsing and the message is fully present from the first moment | — (a defeat is an outcome, not an error) | — (the window cannot be disabled; it is present exactly while the run is defeated and the delay runs) | the run was defeated — the defeat statement and the wave reached are shown | — (one statement and one wave number; never partly shown) |
| S-003 wave reached | — (the line is always present with the statement) | the wave number is read from the counter at the moment of defeat and shown with the statement | — (there is no unknown wave; the run always has a wave in progress) | — (it cannot be disabled separately from the statement) | the defeat message names the wave the player had reached | — (one wave number, never a range) |
| S-003 automatic restart | the run is Playing — no restart pending | the restart delay is elapsing; no input is accepted or needed | — (a failed restart is not a designed state) | — (the restart cannot be cancelled, delayed, or hastened) | the delay ends — the scene restarts into a fresh run at the first wave | — (one restart per defeat) |
| S-001 after the restart | — (the fresh run starts immediately in its Playing state) | — (no loading state is designed for the fresh run) | — | player defeated in the previous attempt — no longer applies; the fresh run's state replaces it | run Playing at the first wave, full health, the firearm equipped, no enemy from the previous attempt | — |

## Notes

- `S-003`'s `loading` value is its lifetime: the restart delay. The message is complete from the first moment; nothing arrives late.
- A defeat is an outcome, not an error. There is no error state for the run ending, and none for the restart.
- The wave reached is F-008's counter frozen at defeat (F-008 FL-043); `S-003` repeats the number the player last saw on `S-001`.
- The restart has no failure state: if it could not complete, that would be a defect, not a designed state.
- The fresh run's own states belong to F-001, F-005, F-008, and F-009; F-010 adds no row for them beyond the one that records the previous attempt is gone.
- A victory does not pass through `S-003`; `S-002`'s states remain F-011's.

# States

Feature F-009 — Boss enemy each wave.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-001 boss entry marker | no boss is entering — no marker | the boss is entering from outside the arena — the marker is visible at it | — (a boss entering is not an error state) | run terminal — no boss enters and no marker appears | the boss has entered — the marker has served its purpose and is gone | — (one boss per wave; the marker is never partly shown) |
| S-001 boss health indicator | no boss is alive — no indicator | — (the indicator appears with the boss, fully populated; there is no loading state) | — (no error state; the boss's health only decreases) | run terminal — the indicator keeps the boss's last remaining health | the boss is alive — the indicator shows its remaining health and decreases as it takes damage | — (one boss at a time; the indicator is never partly shown) |
| S-001 gameplay inputs | no input this step — nothing changes | the boss is entering — the inputs still act | — (no error state; a refused or inert input changes nothing) | player defeated or run terminal — movement, view, switch, and attack change nothing (F-001, F-005) | run Playing — inputs act as F-001, F-002, F-003, and F-004 define; the boss is damaged like any enemy | — (a boss does not partly disable an input) |

## Notes

- The boss health indicator's `empty` value covers both "no boss yet" and "the boss was defeated": in both cases it is absent.
- Its `disabled` value is the terminal run state: it keeps the last remaining health rather than blanking, so the fight's end stays readable.
- The boss's health decreases only through damage; there is no regeneration and no error path.
- The indicator disappears when the boss is defeated and does not carry into the next wave's regular enemies.
- The player's health indicator states remain F-005's, the Attack Cue's remain F-007's, and the wave indicator's remain F-008's; F-009 adds no row for them.

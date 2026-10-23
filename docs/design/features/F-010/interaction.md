# Interaction

Feature F-010 — Defeat and scene restart.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Movement input | unchanged from F-001; inert on `S-001` once the run is defeated, active again in the fresh run after the restart | W, A, S, D — unchanged from F-001 | 1 |
| View control | unchanged from F-001; frozen on `S-001` once the run is defeated, active again after the restart | mouse — unchanged from F-001 | 2 |
| Weapon-switch input | unchanged from F-002; inert on `S-001` once the run is defeated; the fresh run starts with the firearm equipped | Q — unchanged from F-002 | 3 |
| Attack input | unchanged from F-002, F-003, and F-004; inert on `S-001` once the run is defeated, active again after the restart | left mouse button — unchanged from F-002 | 4 |
| Automatic restart | runs on its own after the defeat message has been shown for the restart delay; takes no input, offers no input, and cannot be cancelled, delayed, or hastened by the player | none — system behaviour | not in the focus order |
| Defeat window `S-003` | displays the defeat statement and the wave reached for the restart delay; no interactive element of any kind | none — display only | not in the focus order |

## Notes

- F-010 adds no interactive control and no new keyboard path. `S-003` has nothing to focus, press, or dismiss.
- The player's only part in the restart is the death that causes it. A5 holds by construction: there is no input or confirmation between death and the restarted run.
- The four inputs are inert on `S-001` from the moment the run is defeated (F-001, F-005) until the fresh run begins; the defeat surface does not change that.
- Any input during `S-003` is ignored, not queued: it does not carry into the fresh run and does not skip the delay.
- Focus order continues the shared `S-001` order from F-001 and F-002. `S-003` has no focus order of its own because it has no control.

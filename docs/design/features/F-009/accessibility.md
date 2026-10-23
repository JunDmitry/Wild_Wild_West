# Accessibility

Feature F-009 — Boss enemy each wave.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | a labelled boss health region distinct from the player's health region; the boss's entry and defeat announced once; the boss distinguishable from a regular enemy without relying on colour alone; the marker and indicator never confused with the Attack Cue | "Arena gameplay view" (unchanged from F-001); the boss health region is labelled "Boss health"; the entry marker is labelled "Boss" | application (game view); the boss health region is a status region | W, A, S, D move, mouse rotates the view, Q switches the weapon, left mouse button attacks — unchanged from F-001 and F-002; no tab traversal inside the view | when the boss enters, it is announced once ("Boss enters") at the same moment its health region appears; the region is readable at any time and reads the boss's remaining health; it is not announced on every hit; when the boss is defeated it is announced once ("Boss defeated") and the region disappears; the marker is not re-announced; the terminal state leaves the region on the boss's last remaining health |

## Focus order

- S-001 — Arena gameplay view: focus enters the view; neither the boss marker nor the boss health region is focusable and neither adds a tab stop; Tab order remains F-001's and F-002's — there are none inside the view; movement keys, mouse, Q, and the left mouse button act directly.
- S-002 — Victory window: unchanged from F-011; F-009 adds nothing.

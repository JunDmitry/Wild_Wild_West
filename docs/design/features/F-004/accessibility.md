# Accessibility

Feature F-004 — Melee attack.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | the swing distinguishable from a swing that lands without relying on colour alone; the hit confirmation perceivable at each damaged enemy; no per-frame announcement of the swing | "Arena gameplay view" | application (game view) | left mouse button performs the swing while the melee weapon is equipped and the player is alive; no tab traversal inside the view | the swing is not announced per frame; a landed hit is announced once per impact as a short grouped event stating that the swing hit, not once per damaged enemy; a swing that damages nothing is not announced as a hit; after the run ends, no swing or hit announcement follows; the view holds its last state and is not re-announced |

## Focus order

- S-001 — Arena gameplay view: focus enters the view; there are no tab stops inside it; movement keys, mouse, Q, and the left mouse button act directly. Focus leaves only when the run ends and the defeat/restart or victory surface takes over.
- The swing and the hit confirmation never take focus; they are displays, not controls.
- While the melee weapon is equipped, the firearm's aiming marker (F-003) is absent and is not announced; the melee surface is the swing and its hit confirmation.

# Accessibility

Feature F-002 — Weapon switching.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | a single focusable in-world view with direct keyboard and mouse controls, and a persistent equipped-weapon indicator whose change is announced; the indicator and the player character distinguishable without relying on colour alone | "Arena gameplay view"; the indicator is labelled "Equipped weapon" | application (game view); the indicator is a status region | W, A, S, D move; mouse rotates the view; Q switches the weapon; left mouse button attacks; no tab traversal inside the view | the view is announced as one labelled region on entry, including the current equipped weapon; the equipped weapon is announced again when it changes ("Equipped weapon: Firearm" / "Equipped weapon: Melee Weapon"); movement and attacks are not announced per event; a refused switch changes nothing and is not announced; the terminal state is announced by the defeat/restart or victory surface that follows |

## Focus order

- S-001 — Arena gameplay view: focus enters the view; there are no tab stops inside it; movement keys, mouse, Q, and the left mouse button act directly. Focus leaves only when the run ends and the defeat/restart or victory surface takes over.

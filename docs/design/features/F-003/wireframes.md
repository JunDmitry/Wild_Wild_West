# Wireframes

Feature F-003 — Firearm attack.

### S-001 — Arena gameplay view

```
+----------------------------------------------------------------+
|                                                                |
|                        .  aim marker  .                        |
|                       /                                        |
|                      /  aim line                               |
|                     /                                          |
|                    /                                           |
|                 [ player character ]                           |
|                                                                |
|        [e]  enemy struck — hit confirmation                    |
|             (*) shown at the moment of impact                  |
|                                                                |
|  +------------------------------+        +------------------+  |
|  | equipped weapon: Firearm     |        | Health 100 / 100 |  |
|  +------------------------------+        +------------------+  |
+----------------------------------------------------------------+
  full-screen third-person view from behind the player; a line
  runs from the player along the current View Direction and ends
  in a direction marker; a transient hit confirmation appears at
  the enemy a shot damaged; no HUD reticle, no ammunition count
```

**Structure notes**

- The whole screen is one third-person view of the arena; the aiming marker is drawn in the world, not as a screen-space reticle.
- The aiming marker has two parts: a line from the player character along the current View Direction, and a marker at its far end. Together they show where a shot would travel (R8).
- The marker follows the View Direction continuously while the firearm is equipped and the player is alive. It is absent while the melee weapon is equipped and once the run is terminal.
- The marker is not a targeting reticle and does not select or lock a target: the shot still resolves against the nearest enemy along the direction within Attack Range at impact.
- The hit confirmation is a short-lived mark at the enemy that a shot damaged, appearing at the moment of impact (R9). It appears at the damaged enemy's position, not at a screen corner.
- A shot that reaches no enemy draws no confirmation; the aim line and marker do not change because of a miss.
- The equipped-weapon indicator (F-002) and the health indicator (F-005) stay in their fixed corners. F-003 adds no HUD region and no ammunition display.
- No controls are drawn: movement, view, weapon switch, and attack remain direct inputs on the view, unchanged from F-001 and F-002.

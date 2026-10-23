# Wireframes

Feature F-007 — Enemy pursuit and close-range attack.

### S-001 — Arena gameplay view

```
+----------------------------------------------------------------+
|                                                                |
|                                                                |
|                arena floor and boundary walls                  |
|                                                                |
|                       [e]  attacking enemy                     |
|                        (!) attack cue shown before damage      |
|                                                                |
|                     [ player character ]                       |
|                                                                |
|                                                                |
|  +------------------------------+        +------------------+  |
|  | equipped weapon: Firearm     |        | Health 100 / 100 |  |
|  +------------------------------+        +------------------+  |
+----------------------------------------------------------------+
  full-screen third-person view from behind the player; a nearby
  enemy has closed to attack range and carries a visible cue; the
  cue is shown before the player's health changes; no HUD warning
  banner and no enemy health display
```

**Structure notes**

- The whole screen is one third-person view of the arena; pursuit and the Attack Cue are in-world, not overlays.
- An enemy outside Attack Range is seen moving toward the player. An enemy in range stops closing and starts an attack; at that moment its cue appears (R6).
- The cue is attached to the attacking enemy: a distinct mark at or above that enemy, readable before the health change. It is not a screen-edge flash, a banner, or a sound-only signal.
- The cue stays visible for the wind-up and disappears when the attack resolves or is cancelled. The wind-up's length is a tuning value.
- A missed attack — the player left range, or the enemy was defeated — shows no damage; the cue simply ends with the attack.
- The health indicator (F-005) remains the surface that shows damage; the cue is not a second health display.
- The equipped-weapon indicator (F-002) and the aiming marker (F-003) stay as designed. F-007 adds no HUD region.
- No controls are drawn: movement, view, weapon switch, and attack remain direct inputs on the view, unchanged from F-001 and F-002.

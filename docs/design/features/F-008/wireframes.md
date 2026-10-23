# Wireframes

Feature F-008 — Wave progression.

### S-001 — Arena gameplay view

```
+----------------------------------------------------------------+
|                                                                |
|                     Wave 2 begins                               |
|                                                                |
|                                                                |
|                arena floor and boundary walls                  |
|                                                                |
|                       [e]  attacking enemy                     |
|                        (!) attack cue                          |
|                                                                |
|                     [ player character ]                       |
|                                                                |
|  +------------------------------+        +------------------+  |
|  | equipped weapon: Firearm     |        | Health 100 / 100 |  |
|  +------------------------------+        +------------------+  |
|                                                                |
|  Wave 2 of 3                                                   |
+----------------------------------------------------------------+
  full-screen third-person view from behind the player; the wave
  indicator sits in the HUD beside the weapon and health indicators
  and is always visible; the new-wave announcement appears above the
  arena floor when a wave begins and fades on its own
```

**Structure notes**

- The wave indicator is a persistent HUD region showing the current wave and the total — "Wave 2 of 3". It changes only when a new wave begins (A3, A5).
- The announcement is a transient region, separate from the indicator, shown when a wave begins. It appears at the same moment the indicator advances (A6).
- The announcement is not a pause, a menu, or a countdown; the arena keeps running underneath it, and it is not dismissible by input.
- The first wave is announced at run start, so the announcement region behaves the same for every wave.
- On the third wave the indicator reads "Wave 3 of 3"; clearing it leads to the Victory window `S-002` and the indicator does not advance.
- The equipped-weapon indicator (F-002), the health indicator (F-005), the aiming marker (F-003), and the Attack Cue (F-007) stay as designed. F-008 adds one persistent region and one transient region.
- No controls are drawn: movement, view, weapon switch, and attack remain direct inputs on the view, unchanged from F-001 and F-002.

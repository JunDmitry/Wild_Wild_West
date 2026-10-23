# Wireframes

Feature F-009 — Boss enemy each wave.

### S-001 — Arena gameplay view

```
+----------------------------------------------------------------+
|  BOSS                                                           |
|  +------------------------------------------+                   |
|  |##################################        |  boss health      |
|  +------------------------------------------+                   |
|                                                                |
|                                                                |
|                arena floor and boundary walls                  |
|                                                                |
|                    [B]  boss — distinct entry marker           |
|                     (!) attack cue                             |
|                                                                |
|                     [ player character ]                       |
|                                                                |
|  +------------------------------+        +------------------+  |
|  | equipped weapon: Firearm     |        | Health 100 / 100 |  |
|  +------------------------------+        +------------------+  |
|                                                                |
|  Wave 2 of 3                                                   |
+----------------------------------------------------------------+
  full-screen third-person view from behind the player; the boss
  enters from outside the arena carrying a marker that sets it
  apart from a regular enemy; the boss health indicator sits at the
  top of the screen while the boss is alive and disappears when it
  is defeated
```

**Structure notes**

- The boss is an enemy like any other on the arena floor; its difference on screen is the marker it carries and the health indicator that appears with it.
- The boss entry is F-006's entry with a boss marker: the boss comes from outside the arena and is recognisable as the boss at the moment it enters.
- The boss health indicator is a HUD region at the top of the screen, separate from the player's health indicator at the bottom. It shows remaining health and decreases as the boss takes damage.
- It is labelled "Boss" so it cannot be read as the player's health. It appears when the boss enters and disappears when the boss is defeated; it does not carry into the next wave.
- The Attack Cue (F-007) applies to the boss unchanged: the boss's attack shows the same cue at the boss.
- The equipped-weapon indicator (F-002), the player's health indicator (F-005), the aiming marker (F-003), and the wave indicator and announcement (F-008) stay as designed.
- No controls are drawn: movement, view, weapon switch, and attack remain direct inputs on the view, unchanged from F-001 and F-002.

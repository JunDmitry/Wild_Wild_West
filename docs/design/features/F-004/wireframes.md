# Wireframes

Feature F-004 — Melee attack.

### S-001 — Arena gameplay view

```
+----------------------------------------------------------------+
|                                                                |
|  ==================== boundary wall =========================  |
|                                                                |
|                      (   swing arc   )                         |
|                     /    in front     \                        |
|                    /        of the     \                       |
|                   [e]*      player      [e]*                   |
|                                                                |
|                     [ player character ]                       |
|                                                                |
|  +-------------------------------+       +------------------+  |
|  | equipped weapon: Melee Weapon |       | Health 100 / 100 |  |
|  +-------------------------------+       +------------------+  |
+----------------------------------------------------------------+
  full-screen third-person view from behind the player; the swing
  is an in-world arc in front of the player; a transient hit
  confirmation mark appears at each enemy the swing damages;
  no new HUD region, no menu, and no persistent marker
```

**Structure notes**

- The whole screen is one third-person view of the arena; the player character stays visible within it.
- The swing is an in-world arc in front of the player, occupying the area the melee attack reaches. It appears when the attack starts, spans the attack, and ends with it. It is transient and is not a persistent region.
- A hit confirmation is a transient mark at each enemy whose health the swing reduced, placed at that enemy. Enemies within reach in front carry one; enemies beyond reach or behind the player carry none. A swing that damages nothing shows no mark anywhere.
- No standing aim or reach marker is drawn. The firearm's aiming marker (F-003) is absent while the melee weapon is equipped; the swing is the only melee-specific surface.
- The equipped-weapon indicator (F-002) reads "Melee Weapon" while melee is equipped, and the health indicator (F-005) is unchanged. Both stay in their fixed corners, clear of the player character and of the swing.
- No controls are drawn: movement, view, weapon switch, and attack remain direct inputs on the view, unchanged from F-001 and F-002.

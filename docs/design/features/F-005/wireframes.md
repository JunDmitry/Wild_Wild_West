# Wireframes

Feature F-005 — Player health and death.

### S-001 — Arena gameplay view

```
+----------------------------------------------------------------+
|                                                                |
|                arena floor and boundary walls                  |
|                                                                |
|                                                                |
|                      [ player character ]                      |
|                                                                |
|                                                                |
|  +------------------------------+        +------------------+  |
|  | equipped weapon: Firearm     |        | Health 100 / 100 |  |
|  +------------------------------+        +------------------+  |
+----------------------------------------------------------------+
  full-screen in-world view, seen from behind the player;
  the equipped-weapon indicator (F-002) and the health indicator
  sit in opposite fixed screen corners, clear of the player
  character; no menus or other overlays
```

**Structure notes**

- The whole screen is one third-person view of the arena; the player character stays visible within it, including while defeated.
- The health indicator is a persistent region of the view. It carries the label "Health", the current value against the maximum as a numeric readout, and a fill whose extent is the current value's proportion of the maximum.
- The numeric readout is the exact observation of health; the fill shows the same value structurally. Neither is a control.
- The indicator updates at once when damage is applied. At zero health the readout shows zero and the fill is empty; the indicator does not change again until a new run begins.
- The health indicator occupies the corner opposite the equipped-weapon indicator and stays clear of the player character.
- Movement input, view control, weapon switch, and attack are direct inputs on the view and are unchanged from F-001 and F-002; they are not redrawn as controls here.

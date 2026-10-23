# Wireframes

Feature F-002 — Weapon switching.

### S-001 — Arena gameplay view

```
+--------------------------------------------------------------+
|                                                              |
|              arena floor and boundary walls                  |
|                                                              |
|                                                              |
|                     [ player character ]                     |
|                                                              |
|                                                              |
|  +------------------------------+                            |
|  | equipped weapon: Firearm     |                            |
|  +------------------------------+                            |
+--------------------------------------------------------------+
  full-screen in-world view, seen from behind the player;
  a persistent equipped-weapon indicator sits in a fixed screen
  corner, clear of the player character; no menus or other
  overlays
```

**Structure notes**

- The whole screen is one third-person view of the arena; the player character stays visible within it.
- The equipped-weapon indicator is a persistent region of the view. It names the equipped weapon — "Firearm" or "Melee Weapon" — and updates when a switch succeeds. An icon may accompany the name; its styling belongs to the design system.
- The indicator shows exactly one weapon at a time, matching the one equipped weapon.
- Movement input and view control are unchanged from F-001 and are not redrawn as controls here.
- Weapon-switch input and attack input are direct inputs on the view; neither is an on-screen control to focus first.

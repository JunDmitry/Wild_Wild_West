# Wireframes

Feature F-001 — Player movement and third-person camera.

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
|                                                              |
+--------------------------------------------------------------+
  full-screen in-world view, seen from behind the player;
  no HUD, no menus, no overlays of this feature's own
```

**Structure notes**

- The whole screen is one third-person view of the arena; the player character stays visible within it.
- Arena Bounds are visible as the walls enclosing the floor, so containment is observable in the view itself.
- The view direction runs from behind the player, through the player, into the arena; while the run is Playing, moving the mouse rotates that direction around the player.
- Once the run reaches Defeat or Victory, the view holds the last direction it had at that moment. Mouse movement no longer rotates it, and the player no longer moves. The view itself stays on screen: after Victory, F-011's S-002 window covers its centre; after Defeat, F-010's defeat message appears within the restart delay.
- Input is direct: movement keys and mouse act on the view without any on-screen control to focus first.

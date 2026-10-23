# Wireframes

Feature F-006 — Enemy spawning outside the arena, unseen.

### S-001 — Arena gameplay view

```
+----------------------------------------------------------------+
|                                                                |
|  ==================== boundary wall =========================  |
|                                                                |
|        [e]  entering, just inside the boundary                 |
|                                                                |
|                 arena floor                                    |
|                                                                |
|                     [ player character ]                       |
|                                                                |
|  +------------------------------+        +------------------+  |
|  | equipped weapon: Firearm     |        | Health 100 / 100 |  |
|  +------------------------------+        +------------------+  |
+----------------------------------------------------------------+
  full-screen third-person view from behind the player; the
  boundary wall closes the arena; an entering enemy becomes
  visible only after crossing the boundary inward; no spawn
  effect, flash, or placeholder is drawn at its origin
```

**Spatial structure — the Spawn Band and the current view**

```
            Spawn Band — outside Arena Bounds
   . . . . . . . . . . . . . . . . . . . . . . . . . .
   .            +---------------------------+          .
   .            |                           |          .
   .            |           arena           |          .
   .     [e]    |        [ player ]         |          .
   .            |            \              |          .
   .            |             \ current     |          .
   .            |              \ view       |          .
   .            +---------------------------+          .
   .                         [e]                       .
   . . . . . . . . . . . . . . . . . . . . . . . . . .
   the upper [e] is beyond the far boundary and the lower [e] is
   behind the camera: both are outside the current view. Either
   may be chosen; the Enemy then moves inward and becomes visible
   only after crossing the boundary.
```

**Structure notes**

- The whole screen is one third-person view of the arena; entering enemies are in-world, not overlays. No spawn effect, flash, telegraph, or placeholder is drawn.
- Arena Bounds are the boundary walls enclosing the floor. The Spawn Band is the zone outside them, at arena ground height.
- A spawn position is chosen outside the current view — behind the camera or beyond the far boundary — so the moment of appearing is never on screen.
- An enemy's first visible moment is after it crosses the boundary inward, already moving toward the player. Visibility comes from entering, never from materializing.
- The equipped-weapon indicator (F-002) and the health indicator (F-005) stay in their fixed corners; F-006 adds no HUD region and no counter.
- No controls are drawn: movement, view, weapon switch, and attack remain direct inputs on the view, unchanged from F-001, F-002, and F-005.

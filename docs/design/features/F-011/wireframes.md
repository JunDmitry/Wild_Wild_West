# Wireframes

Feature F-011 — Victory after three waves.

### S-001 — Arena gameplay view

```
+----------------------------------------------------------------+
|                                                                |
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
  full-screen third-person view from behind the player; after
  Victory the arena stops changing behind S-002 — no enemy enters,
  no attack resolves, and neither indicator updates; the window
  covers the centre of this view
```

**Structure notes**

- The whole screen is one third-person view of the arena, unchanged from F-001, F-002, F-005, and F-006.
- After Victory the view is still present behind the window, but nothing in it changes: no enemy enters, no attack resolves, and the equipped-weapon and health indicators hold their last values.
- The player character remains visible where the winning attack left them.
- The view takes no input once the window is shown; S-002 holds the focus.

### S-002 — Victory window

```
+----------------------------------------------------------------+
|                                                                |
|        (S-001 arena view behind, no longer changing)           |
|                                                                |
|        +----------------------------------------------+        |
|        |                  Victory                     |        |
|        +----------------------------------------------+        |
|        |   [ message: the run was won ]               |        |
|        +----------------------------------------------+        |
|                                                                |
+----------------------------------------------------------------+
  a centred window over the arena gameplay view; a title region
  stating Victory and a message region stating the run was won;
  no control, no dismissal, no score, no actions
```

**Structure notes**

- The window is centred over S-001 and is modal: the arena view behind it is visible but takes no input.
- Two content regions only, in reading order: a title region and a message region. There is no list, no score, no button, no link, and no close control.
- The window appears as the direct outcome of the attack that defeats the third Wave's Boss; there is no entry animation, transition, or delay beyond the operation that completes the Wave.
- The window stays for as long as the run is shown. Nothing inside it closes it, and no input dismisses it.
- The window is not a menu: it offers no restart, quit, or continue, and it does not replace the arena view.

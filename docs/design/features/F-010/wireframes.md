# Wireframes

Feature F-010 — Defeat and scene restart.

### S-003 — Defeat window

```
+----------------------------------------------------------------+
|                                                                |
|                                                                |
|                                                                |
|                                                                |
|                    [ defeat statement ]                        |
|                                                                |
|                    [ wave reached ]                            |
|                                                                |
|                                                                |
|                                                                |
|                                                                |
+----------------------------------------------------------------+
  full-screen surface shown once the player's health reaches
  zero; a defeat statement and the wave reached, centred; no
  control, no button, no prompt to continue; the run restarts on
  its own when the restart delay ends
```

**Structure notes**

- One region, centred: a defeat statement, and beneath it the wave reached, taken from the wave counter (F-008) frozen at the moment of defeat.
- Nothing else is on the screen — no score, no statistics, no rewards, no restart control, and no prompt asking the player to act.
- The screen has no interactive element. It is not dismissible, skippable, or confirmable; the restart delay ends it.
- The surface is distinct from `S-002` Victory window: different statement, different outcome, and the two never appear one after the other.
- The message's wording is product's copy; the wireframe fixes the two lines' structure and their order.
- The arena behind the screen is not shown: the defeat surface replaces the view for its lifetime, and the fresh run's `S-001` follows it.

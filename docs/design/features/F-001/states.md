# States

Feature F-001 — Player movement and third-person camera.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-001 movement input | no movement key held — player stationary | — | — | run Defeat or Victory — movement keys produce no position change | run Playing and a key held — player moves relative to the view | key held into Arena Bounds — player stops or slides along the boundary; diagonal input moves no faster than a single direction |
| S-001 view control | mouse still — view unchanged | — | — | run Defeat or Victory — mouse movement changes nothing; the view holds its last direction | run Playing and mouse moved — view rotates around the player | — |

## Notes

- The `disabled` value for both inputs is the terminal run state, not an unavailable control. The run is Defeat or Victory, so the inputs are inert.
- The view's terminal freeze begins in the same operation that ends the run. No state exists between "Playing with the view rotating" and "terminal with the view holding".
- The freeze is cleared only by a new run (F-010's automatic restart), which starts in Playing with the view acting again.
- After Victory, F-011's S-002 window takes the focus; after Defeat, F-010's defeat message appears. Neither changes this feature's input states.

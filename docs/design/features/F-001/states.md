# States

Feature F-001 — Player movement and third-person camera.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-001 movement input | no movement key held — player stationary | — | — | run Defeat or Victory — movement keys produce no position change | run Playing and a key held — player moves relative to the view | key held into Arena Bounds — player stops or slides along the boundary; diagonal input moves no faster than a single direction |
| S-001 view control | mouse still — view unchanged | — | — | run Defeat or Victory — behaviour after the run ends is the open question in FL-003; no view change is claimed here | run Playing and mouse moved — view rotates around the player | — |

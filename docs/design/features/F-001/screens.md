# Screens

Feature F-001 — Player movement and third-person camera.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-001 | Arena gameplay view | show the player character in third person inside the arena and reflect movement and view changes, holding the view still once the run ends | FL-001, FL-002, FL-003 |

## Notes

- S-001 is the only screen F-001 mints. The equipped-weapon, health, and enemy-entry regions added by F-002, F-005, and F-006 are parts of this screen, not screens of their own.
- The terminal state is a state of S-001, not a new screen. Once the run reaches Defeat or Victory, the view holds its last direction and no longer rotates; movement input likewise changes nothing.
- After Victory, F-011's S-002 Victory window covers the centre of the view. After Defeat, F-010's defeat message appears within the restart delay. Both are other features' surfaces; F-001 designs neither.
- The view returns to normal only when a new run begins (F-010's automatic restart).

# Screens

Feature F-005 — Player health and death.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-001 | Arena gameplay view | show the player character in third person inside the arena and reflect movement, view, equipped-weapon, and health changes | FL-009, FL-010, FL-011, FL-012 |

## Notes

- S-001 is the screen F-001 designed and F-002 extended. F-005 adds the health indicator as a second persistent region; it does not mint a new screen id.
- The health indicator is a region of the view, not a screen of its own: no flow can reach a standalone indicator.
- The health indicator sits in the corner opposite the equipped-weapon indicator, clear of the player character.
- The player character remains visible while defeated; F-005 adds no defeat window or restart surface (that is F-010).

# Screens

Feature F-002 — Weapon switching.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-001 | Arena gameplay view | show the player character in third person inside the arena and reflect movement, view, and equipped-weapon changes | FL-004, FL-005, FL-006, FL-007, FL-008 |

## Notes

- S-001 is the screen F-001 designed. F-002 extends it with a persistent equipped-weapon indicator; it does not mint a new screen id.
- The indicator is a region of the view, not a screen of its own: no flow can reach a standalone indicator.
- The third-person character remains visible; the indicator complements the in-world cue rather than replacing it.

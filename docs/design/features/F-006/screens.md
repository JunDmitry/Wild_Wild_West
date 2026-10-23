# Screens

Feature F-006 — Enemy spawning outside the arena, unseen.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-001 | Arena gameplay view | show the player character in third person inside the arena and reflect movement, view, equipped-weapon, health, and the entry of enemies from beyond its bounds | FL-013, FL-014, FL-015, FL-016, FL-017, FL-018 |

## Notes

- S-001 is the screen F-001 designed, F-002 extended with the equipped-weapon indicator, and F-005 extended with the health indicator. F-006 extends it with enemy entry; it does not mint a new screen id.
- Entering enemies are in-world entities observed through the view, not overlays or HUD regions. No flow can reach a standalone entry surface.
- The screen's single job grows by one clause: it now also reflects enemies entering from beyond the arena. The movement, view, weapon, and health clauses are unchanged.
- F-006 adds no counter or indicator. Any wave or remaining-enemy display is F-008's; the entered enemies are counted by the run, not necessarily shown.
- The boundary walls that enclose the arena are the visible Arena Bounds from F-001. The Spawn Band is the zone outside them and is not a separate screen.

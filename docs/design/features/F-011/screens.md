# Screens

Feature F-011 — Victory after three waves.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-001 | Arena gameplay view | show the player character in third person inside the arena and reflect movement, view, equipped-weapon, health, and enemy entry; once the run is won, hold the arena still behind the Victory window | FL-019, FL-020, FL-021, FL-022, FL-023 |
| S-002 | Victory window | announce that the run was won and that it is over | FL-019, FL-022, FL-023 |

## Notes

- S-001 is the screen F-001 designed, extended by F-002, F-005, and F-006. F-011 does not add or move a region on it; it adds the terminal post-victory state in which the arena stops changing.
- S-002 is new. It is the first window in the design: a modal surface shown over S-001.
- S-002's single job is to announce the won run. It does not summarize the run, score it, offer a next step, or close itself.
- The window has a title region and a message region. The title states "Victory" (R2); the message states that the run was won (A2). The wording is product's; the design fixes the two regions and their order.
- The window is not reachable on its own. No flow opens it except the third Wave's completion (FL-019, FL-022), and no control inside it does anything (FL-023).
- F-010 owns defeat and restart; F-011 designs no defeat surface. F-008 owns any wave indicator; F-011 adds none.
- The equipped-weapon and health indicators stay as F-002 and F-005 designed them. They stop updating once the run is terminal.
